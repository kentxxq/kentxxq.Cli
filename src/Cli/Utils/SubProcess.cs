using System.Diagnostics;

namespace Cli.Utils;

public static class SubProcess
{
    public static string Run(string file, params string[] arguments)
    {
        var info = new ProcessStartInfo(file) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        using var process = Process.Start(info) ?? throw new InvalidOperationException("无法启动外部命令。");
        try
        {
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(30000)) throw new TimeoutException("外部命令超时。");
            Task.WhenAll(output, error).GetAwaiter().GetResult();
            if (process.ExitCode != 0) throw new InvalidOperationException($"外部命令失败，退出码 {process.ExitCode}。");
            return output.Result.Trim();
        }
        finally
        {
            if (!process.HasExited) { process.Kill(true); process.WaitForExit(); }
        }
    }
}
