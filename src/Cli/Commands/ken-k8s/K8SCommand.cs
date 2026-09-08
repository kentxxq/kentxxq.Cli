using System.CommandLine;

namespace Cli.Commands.ken_k8s;

public static class K8SCommand
{
    public static readonly Option<string> ConfigPath = new("--kubeconfig", "-c") { Description = "kubeconfig file path" };

    public static readonly Option<string> ClusterNamespace = new("--namespace", "-n") { Description = "specified namespace" };

    public static Command GetCommand()
    {
        var command = new Command("k8s", "get k8s resource info");
        ConfigPath.Recursive = true;
        command.Options.Add(ConfigPath);
        ClusterNamespace.Recursive = true;
        command.Options.Add(ClusterNamespace);

        command.Subcommands.Add(GetRestartPod.GetCommand());
        command.Subcommands.Add(ListUsage.GetCommand());
        return command;
    }
}