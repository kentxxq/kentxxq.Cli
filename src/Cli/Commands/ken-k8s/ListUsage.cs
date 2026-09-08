using System.CommandLine;
using System.Globalization;
using System.Text.Json;
using k8s;
using k8s.Models;
using Spectre.Console;
using Cli.Utils;

namespace Cli.Commands.ken_k8s;

public static class ListUsage
{
    public static Command GetCommand()
    {
        var command = new Command("2", "汇总 Deployment 当前 Pod 的资源用量");
        command.SetAction(async (result, ct) =>
        {
            using var client = new Kubernetes(await ConfigUtils.GetConfig(result.GetValue(K8SCommand.ConfigPath)));
            var table = new Table();
            foreach (var title in new[] { "Namespace", "Deployment", "Replicas", "Memory Usage", "Cpu Usage", "Request Memory", "Limit Memory", "Request Cpu", "Limit Cpu" })
                table.AddColumn(title);
            foreach (var ns in await ConfigUtils.GetNamespaces(client, result.GetValue(K8SCommand.ClusterNamespace), ct))
            {
                var deployments = await client.AppsV1.ListNamespacedDeploymentAsync(ns, cancellationToken: ct);
                var replicaSets = await client.AppsV1.ListNamespacedReplicaSetAsync(ns, cancellationToken: ct);
                var pods = await client.CoreV1.ListNamespacedPodAsync(ns, cancellationToken: ct);
                var usage = new Dictionary<(string Pod, string Container), IDictionary<string, ResourceQuantity>>();
                try
                {
                    var raw = (JsonElement)await client.CustomObjects.GetNamespacedCustomObjectAsync(
                        "metrics.k8s.io", "v1beta1", ns, "pods", string.Empty, cancellationToken: ct);
                    var metrics = raw.Deserialize<PodMetricsList>() ?? throw new JsonException("metrics 响应为空。");
                    foreach (var metric in metrics.Items)
                    foreach (var container in metric.Containers)
                        usage[(metric.Metadata.Name, container.Name)] = container.Usage;
                }
                catch (Exception exception) when (exception is k8s.Autorest.HttpOperationException or HttpRequestException or JsonException
                    || exception is OperationCanceledException && !ct.IsCancellationRequested)
                {
                    MyAnsiConsole.MarkupWarningLine("无法读取 metrics，资源使用率显示 N/A。");
                }
                foreach (var deployment in deployments.Items)
                {
                    var ownedSets = replicaSets.Items.Where(r => r.Metadata.OwnerReferences?.Any(o => o.Kind == "Deployment" && o.Uid == deployment.Metadata.Uid && o.Controller == true) == true)
                        .Select(r => r.Metadata.Uid).ToHashSet();
                    var ownedPods = pods.Items.Where(p => p.Status?.Phase is not ("Succeeded" or "Failed") && p.Metadata.OwnerReferences?.Any(o => o.Kind == "ReplicaSet" && ownedSets.Contains(o.Uid) && o.Controller == true) == true).ToList();
                    var containers = ownedPods.SelectMany(p => p.Spec.Containers.Select(c => (Pod: p.Metadata.Name, Container: c))).ToList();
                    decimal? Total(string resource, bool limit)
                    {
                        if (containers.Count == 0) return null;
                        decimal total = 0;
                        foreach (var entry in containers)
                        {
                            var values = limit ? entry.Container.Resources?.Limits : entry.Container.Resources?.Requests;
                            if (values is null || !values.TryGetValue(resource, out var quantity)) return null;
                            total += quantity.ToDecimal();
                        }
                        return total;
                    }
                    string Percentage(string resource, decimal? limit)
                    {
                        if (limit is null or <= 0) return "N/A";
                        decimal total = 0;
                        foreach (var entry in containers)
                        {
                            if (!usage.TryGetValue((entry.Pod, entry.Container.Name), out var values) || !values.TryGetValue(resource, out var value)) return "N/A";
                            total += value.ToDecimal();
                        }
                        return (total / limit.Value).ToString("P2", CultureInfo.InvariantCulture);
                    }
                    string Amount(decimal? value, bool memory) => value is null ? "N/A" : memory
                        ? (value.Value / 1048576).ToString("0.##", CultureInfo.InvariantCulture) + " MiB"
                        : value.Value.ToString("0.###", CultureInfo.InvariantCulture) + " cores";
                    var lm = Total("memory", true);
                    var lc = Total("cpu", true);
                    table.AddRow(Markup.Escape(ns), Markup.Escape(deployment.Metadata.Name),
                        $"{ownedPods.Count}/{deployment.Spec.Replicas ?? 1}", Percentage("memory", lm), Percentage("cpu", lc),
                        Amount(Total("memory", false), true), Amount(lm, true), Amount(Total("cpu", false), false), Amount(lc, false));
                }
            }
            AnsiConsole.Write(table);
        });
        return command;
    }
}
