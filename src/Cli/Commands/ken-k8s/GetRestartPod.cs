using System.CommandLine;
using k8s;
using Spectre.Console;

namespace Cli.Commands.ken_k8s;

public static class GetRestartPod
{
    public static Command GetCommand()
    {
        var command = new Command("1", "列出发生重启的 Pod 容器");
        command.SetAction(async (result, ct) =>
        {
            var config = await ConfigUtils.GetConfig(result.GetValue(K8SCommand.ConfigPath));
            using var client = new Kubernetes(config);
            var table = new Table().AddColumn("Namespace").AddColumn("Pod Name").AddColumn("Container").AddColumn("Restart Times");
            foreach (var ns in await ConfigUtils.GetNamespaces(client, result.GetValue(K8SCommand.ClusterNamespace), ct))
            {
                var pods = await client.CoreV1.ListNamespacedPodAsync(ns, cancellationToken: ct);
                foreach (var pod in pods.Items)
                foreach (var container in (pod.Status?.ContainerStatuses ?? []).Concat(pod.Status?.InitContainerStatuses ?? []))
                {
                    if (container.RestartCount > 0)
                        table.AddRow(Markup.Escape(ns), Markup.Escape(pod.Metadata.Name), Markup.Escape(container.Name), container.RestartCount.ToString());
                }
            }
            AnsiConsole.Write(table);
            if (table.Rows.Count == 0) Console.WriteLine("未发现已报告重启的容器。");
        });
        return command;
    }
}
