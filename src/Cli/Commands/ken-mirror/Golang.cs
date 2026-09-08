using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public class Golang
{
    private static readonly Option<GolangMirrorEnum> GoLangMirror = new("--mirror", "-m") { Description = $"default {GolangMirrorEnum.Aliyun} registry: https://mirrors.aliyun.com/goproxy/", DefaultValueFactory = _ => GolangMirrorEnum.Aliyun };

    private static readonly string CommandName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "go.exe" : "go";
    
    public static Command GetCommand()
    {
        var command = new Command("golang", "set golang proxy")
        {
            GoLangMirror
        };
        command.SetAction(async (context, cancellationToken) =>
        {
            var golangMirror = context.GetValue(GoLangMirror);
            if (!Enum.IsDefined(golangMirror)) throw new ArgumentException("镜像选项无效。");
            MyAnsiConsole.MarkupSuccessLine($"使用的golang代理为 :{golangMirror}");
            SetGolangMirror(golangMirror);
            await Task.CompletedTask;
        });
        return command;
    }

    private static void SetGolangMirror(GolangMirrorEnum golangMirrorEnum)
    {
        MyLog.Logger?.Debug("golang名称:{CommandName}", CommandName);
        
        var goPath = Finder.FindCommand(CommandName);
        if (!string.IsNullOrEmpty(goPath))
        {
            var config = SubProcess.Run(goPath, "env", "GOENV");
            if (config == "off") throw new InvalidOperationException("GOENV 已关闭，无法保存配置。");
            if (File.Exists(config)) File.Copy(config, config + ".ken-backup-" + Guid.NewGuid().ToString("N"));
            SubProcess.Run(goPath, "env", "-w", "GOPROXY=" + golangMirrorEnum.ToStringFast(useMetadataAttributes: true));

            MyAnsiConsole.MarkupSuccessLine($"验证方法: go env");
        }
        else
        {
            throw new FileNotFoundException("没有找到 go 命令。");
        }
    }
}