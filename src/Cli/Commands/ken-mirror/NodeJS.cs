﻿using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public static class NodeJS
{
    private static readonly Option<NodeJSMirrorEnum> NpmMirror = new(
        new[] { "-m", "--mirror" },
        ()=>NodeJSMirrorEnum.NpmMirror,
        $"default {NodeJSMirrorEnum.NpmMirror} registry: https://registry.npmmirror.com"
    );

    private static readonly string CommandName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "npm.cmd" : "npm";
    
    public static Command GetCommand()
    {
        var command = new Command("nodejs", "set nodejs registry")
        {
            NpmMirror
        };
        command.SetHandler(context =>
        {
            var npmMirror = context.ParseResult.GetValueForOption(NpmMirror);
            MyAnsiConsole.MarkupSuccessLine($"使用的npm镜像为 :{npmMirror}");
            SetNpmMirror(npmMirror);
            return Task.CompletedTask;
        });
        return command;
    }

    private static void SetNpmMirror(NodeJSMirrorEnum nodeJsMirrorEnum)
    {
        MyLog.Logger?.Debug("npm名称:{CommandName}", CommandName);
        
        var npmPath = Finder.FindCommand(CommandName);
        if (!string.IsNullOrEmpty(npmPath))
        {
            var url = nodeJsMirrorEnum.ToStringFast();
            SubProcess.Run(npmPath,$"config set registry {url}");
            MyAnsiConsole.MarkupSuccessLine("验证方法: npm config get registry");
            // TODO 类似于Node-Sass和disturl 之类的资源
        }
        else
        {
            MyAnsiConsole.MarkupWarningLine("没有找到npm命令");
        }
    }
}