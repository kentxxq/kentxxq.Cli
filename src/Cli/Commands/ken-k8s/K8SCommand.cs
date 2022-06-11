using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cli.Utils;
using k8s;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;

namespace Cli.Commands.ken_k8s;

public static class K8SCommand
{
    public static readonly Option<string> ConfigPath = new(
        new[] { "-c", "--kubeconfig" },
        "kubeconfig file path"
        );

    public static readonly Option<string> ClusterNamespace = new(
        new[] { "-n", "--namespace" }, 
        "specified namespace"
        );

    public static Command GetCommand()
    {
        var command = new Command("k8s", "get k8s resource info");
        command.AddGlobalOption(ConfigPath);
        command.AddGlobalOption(ClusterNamespace);
        
        command.AddCommand(GetRestartPod.GetCommand());
        command.AddCommand(ListUsage.GetCommand());
        return command;
    }
}