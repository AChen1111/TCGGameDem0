using System;
using System.Diagnostics;
using System.Text;

/// <summary>Editor 内启动外部进程的公共封装: 重定向输出、终止与存活判断.</summary>
public static class EditorProcessRunner
{
    /// <summary>创建已配置重定向的进程(未启动). onOutput 在后台线程被调用.</summary>
    public static Process Create(string fileName, string arguments, string workingDirectory, Action<string> onOutput)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };
        process.OutputDataReceived += (_, args) => onOutput?.Invoke(args.Data);
        process.ErrorDataReceived += (_, args) => onOutput?.Invoke(args.Data);
        return process;
    }

    public static void StartRedirected(Process process)
    {
        if (!process.Start())
        {
            throw new InvalidOperationException("系统拒绝创建进程：" + process.StartInfo.FileName);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    /// <summary>终止并释放进程; 返回是否真的终止了一个存活进程. 失败原因通过 error 返回.</summary>
    public static bool Stop(ref Process process, int waitMilliseconds, out int processId, out string error)
    {
        Process target = process;
        process = null;
        processId = -1;
        error = null;
        if (!IsAlive(target))
        {
            target?.Dispose();
            return false;
        }

        try
        {
            processId = target.Id;
            target.Kill();
            target.WaitForExit(waitMilliseconds);
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
        finally
        {
            target.Dispose();
        }
    }

    public static void Dispose(ref Process process)
    {
        Process target = process;
        process = null;
        target?.Dispose();
    }

    public static bool IsAlive(Process process)
    {
        if (process == null)
        {
            return false;
        }

        try
        {
            process.Refresh();
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    public static int SafeExitCode(Process process)
    {
        try
        {
            return process?.ExitCode ?? -1;
        }
        catch
        {
            return -1;
        }
    }

    public static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }
}
