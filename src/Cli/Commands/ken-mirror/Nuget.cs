using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public static class Nuget
{
    private static readonly Option<NugetMirrorEnum> NugetMirror = new(
        new[] { "-m", "--mirror" },
        ()=>NugetMirrorEnum.Huawei,
        "default huawei registry: https://mirrors.cloud.tencent.com/nuget/"
    );

    private static readonly string CommandName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
    
    public static Command GetCommand()
    {
        var command = new Command("nuget", "set nuget mirror")
        {
            NugetMirror
        };
        command.SetHandler(context =>
        {
            var nugetMirror = context.ParseResult.GetValueForOption(NugetMirror);
            MyAnsiConsole.MarkupSuccessLine($"使用的nuget源为 :{nugetMirror}");
            SetNpmMirror(nugetMirror);
            return Task.CompletedTask;
        });
        return command;
    }

    private static void SetNpmMirror(NugetMirrorEnum nugetMirrorEnum)
    {
        MyLog.Logger?.Debug("nuget名称:{CommandName}", CommandName);
        
        var nugetPath = Finder.FindCommand(CommandName);
        if (!string.IsNullOrEmpty(nugetPath))
        {
            var url = nugetMirrorEnum.ToStringFast();
            SubProcess.Run(nugetPath,$"nuget add source --name {nugetMirrorEnum} {url}");
            
            SubProcess.Run(nugetPath,$"nuget update source --name {nugetMirrorEnum} {url}");
            
            SubProcess.Run(nugetPath,"nuget list source");
            
            MyAnsiConsole.MarkupSuccessLine("你应该通过 dotnet nuget enable/disable source source_name 来指定使用的源");
        }
        else
        {
            MyAnsiConsole.MarkupWarningLine("没有找到npm命令");
        }
    }
}