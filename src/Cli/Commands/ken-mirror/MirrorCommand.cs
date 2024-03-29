﻿using System;
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

        command.AddCommand(NodeJS.GetCommand());
        command.AddCommand(Golang.GetCommand());
        command.AddCommand(DockerHub.GetCommand());
        command.AddCommand(Nuget.GetCommand());
        command.AddCommand(Python.GetCommand());
        command.AddCommand(Java.GetCommand());
        return command;
    }
}