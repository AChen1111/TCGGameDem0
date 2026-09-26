using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public static class DevelopmentInputs
{
    public static string Files(IEnumerable<string> paths)
    {
        var text = new StringBuilder();
        foreach (string path in paths.Select(Path.GetFullPath).Distinct().OrderBy(x => x, StringComparer.Ordinal))
            if (File.Exists(path)) text.Append(path).Append(':').Append(DevelopmentPackage.HashFile(path)).Append('\n');
        return CodeUpdate.Sha256Of(Encoding.UTF8.GetBytes(text.ToString()));
    }
    public static IEnumerable<string> Under(string path) => Directory.Exists(path) ?
        Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Where(x =>
            !x.Replace('\\', '/').Split('/').Any(part => part == "bin" || part == "obj" || part == ".git")) : Array.Empty<string>();
    public static string Backend() => Files(Under("Backend/src").Where(x => x.EndsWith(".cs") || x.EndsWith(".csproj") ||
        Path.GetFileName(x).StartsWith("appsettings") || x.EndsWith(".cshtml") || x.EndsWith(".props") || x.EndsWith(".targets"))
        .Concat(Under("Assets/Shared")).Concat(Under("Backend").Where(x => x.EndsWith(".props") || x.EndsWith(".targets") || x.EndsWith("global.json"))));
    public static string Hot() => Files(CompilationPipeline.GetAssemblies(AssembliesType.Player)
        .Where(x => x.name == "HotUpdate").SelectMany(x => x.sourceFiles)
        .Concat(new[] { "Assets/Scripts/HotUpdate.asmdef", "Packages/manifest.json", "Packages/packages-lock.json", "ProjectSettings/ProjectSettings.asset" }));
    public static string Resources() => Files(Under("Assets").Where(x =>
        !x.Replace('\\', '/').StartsWith("Assets/StreamingAssets/") &&
        !x.Replace('\\', '/').StartsWith("Assets/HybridCLRGenerate/") &&
        !x.Replace('\\', '/').StartsWith("Assets/Editor/") &&
        !x.Replace('\\', '/').StartsWith("Assets/AddressableAssetsData/Android/") &&
        !x.Replace('\\', '/').StartsWith("Assets/AddressableAssetsData/StandaloneWindows64/") &&
        !x.EndsWith(".cs") && !x.EndsWith(".cs.meta") && !x.EndsWith(".dll") && !x.EndsWith(".dll.meta"))
        .Concat(new[] { "ProjectSettings/ProjectSettings.asset", "ProjectSettings/GraphicsSettings.asset", "Packages/packages-lock.json" }));
    public static string Apk() => PlayerFiles(true);
    public static string PlayerSource() => PlayerFiles(false);
    static string PlayerFiles(bool includeGenerated)
    {
        var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player).Where(x => x.name != "HotUpdate");
        var scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
        var files = assemblies.SelectMany(x => x.sourceFiles)
            .Where(x => !x.Contains("AOTGenericReferences"))
            .Concat(AssetDatabase.GetDependencies(scenes, true))
            .Concat(Under("Assets/Plugins"))
            .Concat(Under("Assets").Where(x => x.EndsWith("link.xml") || x.EndsWith(".asmdef") || x.EndsWith(".asmref")))
            .Concat(Under("Assets").Where(x => x.Replace('\\', '/').Contains("/Resources/")))
            .Concat(Under("Assets/StreamingAssets").Where(x => !x.EndsWith("HotUpdate.dll.bytes") && !x.EndsWith("development-apk.txt") && !x.EndsWith(".meta")))
            .Concat(Under("ProjectSettings"))
            .Concat(new[] { "Packages/manifest.json", "Packages/packages-lock.json" });
        if (!includeGenerated)
            files = files.Where(x => !x.Replace('\\', '/').StartsWith("Assets/HybridCLRGenerate/") &&
                !x.Replace('\\', '/').StartsWith("Assets/StreamingAssets/HybridCLR/"));
        return Files(files);
    }
    // 外部类型/成员/泛型以及委托签名改变时重建壳包; 不依据方法实现字节判断.
    public static string BridgeRequirements(string dll)
    {
        using (var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(dll))
        {
            var values = new List<string>();
            foreach (var module in assembly.Modules)
            {
                values.AddRange(module.AssemblyReferences.Select(x => x.FullName));
                values.AddRange(module.GetTypeReferences().Select(x => x.FullName));
                values.AddRange(module.GetMemberReferences().Select(x => x.FullName));
                foreach (var type in AllTypes(module.Types))
                {
                    if (type.BaseType != null) values.Add(type.BaseType.FullName);
                    values.AddRange(type.Interfaces.Select(x => x.InterfaceType.FullName));
                    values.AddRange(type.Fields.Select(x => x.FieldType.FullName));
                    foreach (var method in type.Methods)
                    {
                        values.Add(method.ReturnType.FullName + "(" + string.Join(",", method.Parameters.Select(x => x.ParameterType.FullName)) + ")");
                        foreach (var attr in method.CustomAttributes) values.Add(attr.AttributeType.FullName);
                        if (!method.HasBody) continue;
                        bool reflection = method.Body.Instructions.Any(x => x.Operand is Mono.Cecil.MethodReference call &&
                            (call.DeclaringType.Namespace.StartsWith("System.Reflection") || call.DeclaringType.FullName == "System.Type"));
                        if (reflection) values.AddRange(method.Body.Instructions.Where(x => x.Operand is string).Select(x => "reflection:" + x.Operand));
                        foreach (var instruction in method.Body.Instructions)
                        {
                            if (instruction.Operand is Mono.Cecil.GenericInstanceMethod generic) values.Add(generic.FullName);
                            if (instruction.Operand is Mono.Cecil.TypeReference reference) values.Add(reference.FullName);
                        }
                    }
                }
            }
            return CodeUpdate.Sha256Of(Encoding.UTF8.GetBytes(string.Join("\n", values.Distinct().OrderBy(x => x, StringComparer.Ordinal))));
        }
    }
    static IEnumerable<Mono.Cecil.TypeDefinition> AllTypes(IEnumerable<Mono.Cecil.TypeDefinition> types)
    {
        foreach (var type in types) { yield return type; foreach (var child in AllTypes(type.NestedTypes)) yield return child; }
    }
}
