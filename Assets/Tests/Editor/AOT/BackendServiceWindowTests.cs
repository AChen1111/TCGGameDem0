using System.Diagnostics;
using System.Reflection;
using System.Text;
using NUnit.Framework;

public sealed class BackendServiceWindowTests
{
    [Test]
    public void Backend_process_reads_redirected_output_as_utf8()
    {
        MethodInfo createProcess = typeof(BackendServiceController).GetMethod(
            "CreateProcess",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(createProcess);
        using var process = (Process)createProcess.Invoke(
            null,
            new object[] { "dotnet", "--version", ".", "TEST" });

        Assert.AreEqual(Encoding.UTF8, process.StartInfo.StandardOutputEncoding);
        Assert.AreEqual(Encoding.UTF8, process.StartInfo.StandardErrorEncoding);
    }
}
