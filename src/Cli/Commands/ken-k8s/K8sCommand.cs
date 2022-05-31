using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cli.Utils;
using k8s;
using Spectre.Console;

namespace Cli.Commands.ken_k8s;

public class K8sCommand
{
    private static readonly Argument<string> SubCommand = new("get-restarted-pod", "get-restarted-pod");

    private static readonly Option<string> ConfigPath = new(new[] { "-c", "--kubeconfig" }, () => "", "default $HOME/.kube/config");

    private Kubernetes _kubernetes;

    public Command GetCommand()
    {
        var command = new Command("k8s", "k8s")
        {
            SubCommand,
            ConfigPath
        };
        command.SetHandler<string,string>(Run, SubCommand,ConfigPath);
        return command;
    }

    private async Task Run(string subCommand, string configPath)
    {
        var config = configPath == ""
            ? KubernetesClientConfiguration.BuildConfigFromConfigFile()
            : await KubernetesClientConfiguration.BuildConfigFromConfigFileAsync(new FileInfo(configPath));
        _kubernetes = new Kubernetes(config);
        switch (subCommand)
        {
            case "get-restarted-pod":
                await GetRestartedPod();
                return;
            default:
                MyAnsiConsole.MarkupErrorLine("command not found!");
                return;;
        }
    }

    private async Task GetRestartedPod()
    {
        var table = new Table();
        await AnsiConsole.Live(table).StartAsync(async ctx =>
        {
            table.AddColumn("Pod Name");
            table.AddColumn("Restart Times");
            ctx.Refresh();

            var namespaces = await _kubernetes.ListNamespaceAsync();
            foreach (var ns in namespaces.Items)
            {
                MyAnsiConsole.MarkupSuccessLine($"{ns.Metadata.Name}");
                var pods = await _kubernetes.ListNamespacedPodAsync(ns.Metadata.Name);
                var restartedPods = pods.Items.Where(p => p.Status.ContainerStatuses.Any(c => c.RestartCount != 0));
                foreach (var restartedPod in restartedPods)
                {
                    foreach (var c in restartedPod.Status.ContainerStatuses)
                    {
                        table.AddRow(c.Name, c.RestartCount.ToString());
                    }
                }
            }
        });
    }
}