using System;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
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
        // 表头
        table.AddColumn("Namespace");
        table.AddColumn("Deployment");
        table.AddColumn("Memory Usage");
        table.AddColumn("Cpu Usage");
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
                // deployment信息
                var dList = await client.ListNamespacedDeploymentAsync(ns.Metadata.Name);
                // metrics信息
                var mList = await client.GetKubernetesPodsMetricsByNamespaceAsync(ns.Metadata.Name);

                foreach (var d in dList.Items)
                {
                    // 获取资源的配置信息
                    ResourceQuantity? rm = null;
                    ResourceQuantity? rc = null;
                    ResourceQuantity? lm = null;
                    ResourceQuantity? lc = null;
                    var c = d.Spec.Template.Spec.Containers.First();
                    c.Resources.Requests?.TryGetValue("memory", out rm);
                    c.Resources.Limits?.TryGetValue("memory", out lm);
                    c.Resources.Requests?.TryGetValue("cpu", out rc);
                    c.Resources.Limits?.TryGetValue("cpu", out lc);

                    // 使用率计算
                    decimal memoryUsage = 0;
                    decimal cpuUsage = 0;
                    if (lm is not null)
                    {
                        var metrics = mList.Items.Where(p => p.Metadata.Name.StartsWith(d.Metadata.Name)).ToList();
                        var mUsage = metrics.First().Containers.First().Usage["memory"].ToDecimal();
                        memoryUsage = mUsage / lm.ToDecimal();
                    }
                    if (lc is not null)
                    {
                        var metrics = mList.Items.Where(p => p.Metadata.Name.StartsWith(d.Metadata.Name)).ToList();
                        var cUsage = metrics.First().Containers.First().Usage["cpu"].ToDecimal();
                        cpuUsage = cUsage / lc.ToDecimal();
                    }
                    
                    
                    table.AddRow(d.Metadata.NamespaceProperty, d.Metadata.Name,
                        $"{memoryUsage:P2}",
                        $"{cpuUsage:P2}",
                        rm?.Value ?? "",
                        lm?.Value ?? "",
                        rc?.Value ?? "",
                        lc?.Value ?? ""
                    );
                    ctx.Refresh();
                }
            }
        });
        
    }
}