using System.Diagnostics;
using System.Text;
using NUnit.Framework;

public sealed class EditorProcessRunnerTests
{
    [Test]
    public void Created_process_reads_redirected_output_as_utf8()
    {
        using Process process = EditorProcessRunner.Create("dotnet", "--version", ".", null);

        Assert.AreEqual(Encoding.UTF8, process.StartInfo.StandardOutputEncoding);
        Assert.AreEqual(Encoding.UTF8, process.StartInfo.StandardErrorEncoding);
        Assert.IsTrue(process.StartInfo.RedirectStandardOutput);
        Assert.IsTrue(process.StartInfo.RedirectStandardError);
    }
}
