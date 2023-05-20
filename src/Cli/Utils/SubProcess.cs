using System.Diagnostics;

namespace Cli.Utils;

public class SubProcess
{
    public static void Run(string file,string args)
    {
        MyAnsiConsole.MarkupSuccessLine($"执行命令:{file} {args}");
        var process = Process.Start(file,args);
        process.WaitForExit();
        if (process.ExitCode == 0)
        {
            MyAnsiConsole.MarkupSuccessLine("执行成功");
        }
        else
        {
            MyAnsiConsole.MarkupErrorLine("执行失败");
        }
        MyLog.Logger?.Debug("退出状态码:{ProcessExitCode}", process.ExitCode);
        MyLog.Logger?.Debug("退出时间:{ProcessExitTime}", process.ExitTime);
    }
}