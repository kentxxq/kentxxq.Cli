using System;
using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public static class Python
{
    private static readonly Option<PythonMirrorEnum> PythonMirror = new(
        new[] { "-m", "--mirror" },
        ()=>PythonMirrorEnum.Aliyun,
        $"default {PythonMirrorEnum.Aliyun} registry: https://mirrors.aliyun.com/pypi/simple/"
    );

    private static readonly string CommandName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "pip.exe" : "pip";
    
    public static Command GetCommand()
    {
        var command = new Command("python", "set pip mirror")
        {
            PythonMirror
        };
        command.SetHandler(context =>
        {
            var pythonMirror = context.ParseResult.GetValueForOption(PythonMirror);
            MyAnsiConsole.MarkupSuccessLine($"使用的nuget源为 :{pythonMirror}");
            SetPythonMirror(pythonMirror);
            return Task.CompletedTask;
        });
        return command;
    }

    private static void SetPythonMirror(PythonMirrorEnum pythonMirrorEnum)
    {
        MyLog.Logger?.Debug("nuget名称:{CommandName}", CommandName);
        
        var pipPath = Finder.FindCommand(CommandName);
        if (!string.IsNullOrEmpty(pipPath))
        {
            var url = pythonMirrorEnum.ToStringFast();
            var domain = new Uri(url).Host;
            SubProcess.Run(pipPath,$"config set global.index-url \"{url}\" ");
            
            SubProcess.Run(pipPath,$"config set global.trusted-host \"{domain}\" ");
        }
        else
        {
            MyAnsiConsole.MarkupWarningLine("没有找到pip命令");
        }
    }
}