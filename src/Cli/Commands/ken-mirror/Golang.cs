﻿using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public class Golang
{
    private static readonly Option<GolangMirrorEnum> GoLangMirror = new(
        new[] { "-m", "--mirror" },
        ()=>GolangMirrorEnum.Aliyun,
        $"default {GolangMirrorEnum.Aliyun} registry: https://mirrors.aliyun.com/goproxy/"
    );

    private static readonly string CommandName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "go.exe" : "go";
    
    public static Command GetCommand()
    {
        var command = new Command("golang", "set golang proxy")
        {
            GoLangMirror
        };
        command.SetHandler(context =>
        {
            var golangMirror = context.ParseResult.GetValueForOption(GoLangMirror);
            MyAnsiConsole.MarkupSuccessLine($"使用的golang代理为 :{golangMirror}");
            SetGolangMirror(golangMirror);
            return Task.CompletedTask;
        });
        return command;
    }

    private static void SetGolangMirror(GolangMirrorEnum golangMirrorEnum)
    {
        MyLog.Logger?.Debug("golang名称:{CommandName}", CommandName);
        
        var goPath = Finder.FindCommand(CommandName);
        if (!string.IsNullOrEmpty(goPath))
        {
            var url = golangMirrorEnum.ToStringFast();
            var sumdb = golangMirrorEnum == GolangMirrorEnum.Default ? "sum.golang.org" : "sum.golang.google.cn";
            SubProcess.Run(goPath,$"env -w GOPROXY={url}");
            
            SubProcess.Run(goPath,$"env -w GO111MODULE=on");
            
            SubProcess.Run(goPath,$"env -w GOSUMDB={sumdb}");

            MyAnsiConsole.MarkupSuccessLine($"验证方法: go env");
        }
        else
        {
            MyAnsiConsole.MarkupWarningLine("没有找到go命令");
        }
    }
}