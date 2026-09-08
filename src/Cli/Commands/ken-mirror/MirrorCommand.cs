using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Masuit.Tools;

namespace Cli.Commands.ken_mirror;

/// <summary>
/// 镜像相关的子命令
/// </summary>
public class MirrorCommand
{
    public static Command GetCommand()
    {
        var command = new Command("mirror", "set target mirror");

        command.Subcommands.Add(NodeJS.GetCommand());
        command.Subcommands.Add(Golang.GetCommand());
        command.Subcommands.Add(DockerHub.GetCommand());
        command.Subcommands.Add(Nuget.GetCommand());
        command.Subcommands.Add(Python.GetCommand());
        command.Subcommands.Add(Java.GetCommand());
        return command;
    }
}