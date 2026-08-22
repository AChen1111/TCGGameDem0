using System.IO;
using System.Text;
using NUnit.Framework;

public class LuaPadBrowserTests
{
    [Test]
    public void BuildStartArguments_PassesPortAndUrlOnly()
    {
        Assert.AreEqual(
            "--port 123 --url http://127.0.0.1:4567/",
            LuaPadBrowser.BuildStartArguments("http://127.0.0.1:4567", 123));
    }

    [Test]
    public void TryWriteLine_OnWriteFailure_ReturnsFalse()
    {
        var writer = new StreamWriter(new ThrowOnWriteStream(), new UTF8Encoding(false)) { AutoFlush = true };
        Assert.IsFalse(LuaPadBrowser.TryWriteLine(writer, "{}"));
    }

    sealed class ThrowOnWriteStream : MemoryStream
    {
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return;
            }
            throw new IOException("Unable to write data to the transport connection");
        }

        public override void Write(System.ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length == 0)
            {
                return;
            }
            throw new IOException("Unable to write data to the transport connection");
        }
    }
}
