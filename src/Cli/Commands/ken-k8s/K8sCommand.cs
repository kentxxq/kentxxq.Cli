using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cli.Utils;
using k8s;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;

namespace Cli.Commands.ken_k8s;

public class K8sCommand
{
    private static readonly Argument<string> SubCommand = new("get-restarted-pod", "get-restarted-pod");

    private static readonly Option<string> ConfigPath = new(new[] { "-c", "--kubeconfig" }, () => "", "default $HOME/.kube/config");
    
    private static readonly Option<string> ClusterNamespace = new(new[] { "-n", "--namespace" }, () => "", "default all namespace");


    private Kubernetes _kubernetes;

    public Command GetCommand()
    {
        var command = new Command("k8s", "k8s")
        {
            SubCommand,
            ConfigPath,
            ClusterNamespace
        };
        command.SetHandler<string,string,string>(Run, SubCommand,ConfigPath,ClusterNamespace);
        return command;
    }

    private async Task Run(string subCommand, string configPath,string clusterNamespace)
    {
        var config = configPath == ""
            ? KubernetesClientConfiguration.BuildConfigFromConfigFile()
            : await KubernetesClientConfiguration.BuildConfigFromConfigFileAsync(new FileInfo(configPath));
        _kubernetes = new Kubernetes(config);
        switch (subCommand)
        {
            case "get-restarted-pod":
                await GetRestartedPod(clusterNamespace);
                return;
            default:
                MyAnsiConsole.MarkupErrorLine("command not found!");
                return;;
        }
    }

    private async Task GetRestartedPod(string clusterNamespace)
    {
        var table = new Table();
        await AnsiConsole.Live(table).StartAsync(async ctx =>
        {
            var namespaces = await _kubernetes.ListNamespaceAsync();
            if (!clusterNamespace.IsNullOrEmpty())
            {
                namespaces.Items = namespaces.Items.Where(n => n.Metadata.Name == clusterNamespace).ToList();
            }
            
            foreach (var ns in namespaces.Items)
            {
                var pods = await _kubernetes.ListNamespacedPodAsync(ns.Metadata.Name);
                var restartedPods = pods.Items.Where(p => p.Status.ContainerStatuses.Any(c => c.RestartCount != 0));
                foreach (var restartedPod in restartedPods)
                {
                    foreach (var c in restartedPod.Status.ContainerStatuses)
                    {
                        if (table.Rows.Count == 0)
                        {
                            table.AddColumn("Namespace");
                            table.AddColumn("Pod Name");
                            table.AddColumn("Restart Times");
                            ctx.Refresh();
                        }
                        table.AddRow(ns.Metadata.Name,c.Name, c.RestartCount.ToString());
                    }
                }
            }
            if (table.Rows.Count == 0)
            {
                table.AddColumn("Not Found! Your pods are healthy.");
            }
        });
        
    }
}