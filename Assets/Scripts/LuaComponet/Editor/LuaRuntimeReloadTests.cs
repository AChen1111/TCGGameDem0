using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using XLua;

public class LuaRuntimeReloadTests
{
    [Test]
    public void RuntimeReload_AfterFailedRequire_ReloadsWhenSourceIsFixed()
    {
        bool breakProbe = false;
        var env = new LuaEnv();
        string root = Application.dataPath + "/Scripts/LuaRaw/";
        env.AddLoader((ref string filepath) =>
        {
            string name = filepath;
            int lastDot = name.LastIndexOf('.');
            if (lastDot >= 0)
            {
                name = name.Substring(lastDot + 1);
            }

            if (name == "ReloadProbe")
            {
                string src = breakProbe
                    ? "1"
                    : "ReloadProbe = {}\nReloadProbe.__index = ReloadProbe\n";
                return Encoding.UTF8.GetBytes(src);
            }

            string[] files = Directory.GetFiles(root, name + ".lua", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                return null;
            }
            filepath = files[0];
            return File.ReadAllBytes(files[0]);
        });

        try
        {
            env.DoString("require 'Main'");
            env.DoString(@"
ReloadProbe = {}
ReloadProbe.__index = ReloadProbe
moduleList['ReloadProbe'] = ReloadProbe
package.loaded['ReloadProbe'] = ReloadProbe
");

            breakProbe = true;
            Assert.Throws<LuaException>(() => env.DoString("Main.runtimeReload('ReloadProbe')"));

            breakProbe = false;
            env.DoString("Main.runtimeReload('ReloadProbe')");
            using (LuaTable probe = env.Global.Get<LuaTable>("ReloadProbe"))
            {
                Assert.IsNotNull(probe);
            }
        }
        finally
        {
            env.Dispose();
        }
    }
}
