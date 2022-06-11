using System;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using k8s;
using Masuit.Tools;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;

namespace Cli.Commands.ken_k8s;

public static class ListUsage
{
    public static Command GetCommand()
    {
        var command = new Command("2", "list restarted pod");
        command.SetHandler(async context =>
        {
            var configPath = context.ParseResult.GetValueForOption(K8SCommand.ConfigPath)?.Replace(" ", "");
            var clusterNamespace = context.ParseResult.GetValueForOption(K8SCommand.ClusterNamespace);

            var config = await ConfigUtils.GetConfig(configPath);
            var client = new Kubernetes(config);
            await PrintDeployUsageTable(client, clusterNamespace);
        });
        return command;
    }

    private static async Task PrintDeployUsageTable(Kubernetes client, string? clusterNamespace)
    {
        var table = new Table();
        table.AddColumn("Namespace");
        table.AddColumn("Deployment");
        table.AddColumn("Memory Usage");
        table.AddColumn("Request Memory");
        table.AddColumn("Limit Memory");
        table.AddColumn("Request Cpu");
        table.AddColumn("Limit Cpu");
        await AnsiConsole.Live(table).StartAsync(async ctx =>
        {
            ctx.Refresh();
            var namespaces = await client.ListNamespaceAsync();
            if (!CollectionUtilities.IsNullOrEmpty(clusterNamespace))
                namespaces.Items = namespaces.Items.Where(n => n.Metadata.Name == clusterNamespace).ToList();

            foreach (var ns in namespaces.Items)
            {
                var dList = await client.ListNamespacedDeploymentAsync(ns.Metadata.Name);
                foreach (var d in dList.Items)
                {
                    table.AddRow(d.Metadata.NamespaceProperty, d.Metadata.Name,
                        "100%",// TODO 计算。还有资源使用率的排序。。eventcommand
                        d.Spec.Template.Spec.Containers.First().Resources.Requests["memory"].Value,
                        d.Spec.Template.Spec.Containers.First().Resources.Limits["memory"].Value,
                        d.Spec.Template.Spec.Containers.First().Resources.Requests["cpu"].Value,
                        d.Spec.Template.Spec.Containers.First().Resources.Limits["cpu"].Value);
                    ctx.Refresh();
                }
            }
        });
        
    }
}