using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using k8s;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;

namespace Cli.Commands.ken_k8s;

public static class GetRestartPod
{
    public static Command GetCommand()
    {
        var command = new Command("1", "list restarted pod");
        command.SetHandler(async context =>
        {
            var configPath = context.ParseResult.GetValueForOption(K8SCommand.ConfigPath)?.Replace(" ","");
            var clusterNamespace = context.ParseResult.GetValueForOption(K8SCommand.ClusterNamespace)?.Replace(" ","");
            
            var config = await ConfigUtils.GetConfig(configPath);
            var client = new Kubernetes(config);
            await PrintRestartedPod(client, clusterNamespace);
        });
        return command;
    }
    
    private static async Task PrintRestartedPod(Kubernetes client, string? clusterNamespace)
    {
        var table = new Table();
        await AnsiConsole.Live(table).StartAsync(async ctx =>
        {
            var namespaces = await client.ListNamespaceAsync();
            if (!clusterNamespace.IsNullOrEmpty())
                namespaces.Items = namespaces.Items.Where(n => n.Metadata.Name == clusterNamespace).ToList();

            foreach (var ns in namespaces.Items)
            {
                var pods = await client.ListNamespacedPodAsync(ns.Metadata.Name);
                var restartedPods = pods.Items.Where(p => p.Status.ContainerStatuses.Any(c => c.RestartCount != 0));
                foreach (var restartedPod in restartedPods)
                foreach (var c in restartedPod.Status.ContainerStatuses)
                {
                    if (table.Rows.Count == 0)
                    {
                        table.AddColumn("Namespace");
                        table.AddColumn("Pod Name");
                        table.AddColumn("Restart Times");
                        ctx.Refresh();
                    }

                    table.AddRow(ns.Metadata.Name, c.Name, c.RestartCount.ToString());
                }
            }

            if (table.Rows.Count == 0) table.AddColumn("Not Found! Your pods are healthy.");
        });
    }
}