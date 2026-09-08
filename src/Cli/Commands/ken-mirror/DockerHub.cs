using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public static class DockerHub
{
    private static readonly Option<DockerHubMirrorEnum> DockerHubMirror = new("--mirror", "-m") { Description = $"default {DockerHubMirrorEnum.NetEase163} registry: https://hub-mirror.c.163.com", DefaultValueFactory = _ => DockerHubMirrorEnum.NetEase163 };

    private static readonly string ConfigPath =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),@".docker\daemon.json") : "/etc/docker/daemon.json";

    private const string MirrorFieldName = "registry-mirrors";
    
    public static Command GetCommand()
    {
        var command = new Command("dockerhub", "set dockerhub mirror registry")
        {
            DockerHubMirror
        };
        command.SetAction(async (context, cancellationToken) =>
        {
            var dockerHubMirror = context.GetValue(DockerHubMirror);
            if (!Enum.IsDefined(dockerHubMirror)) throw new ArgumentException("镜像选项无效。");
            MyAnsiConsole.MarkupSuccessLine($"使用的dockerHub镜像为 :{dockerHubMirror}");
            await SetDockerhubMirror(dockerHubMirror);
        });
        return command;
    }

    private static async Task SetDockerhubMirror(DockerHubMirrorEnum mirror)
    {
        await WriteMirror(ConfigPath, mirror);
        MyAnsiConsole.MarkupSuccessLine("Docker 镜像配置已保存，请自行重启 Docker。");
    }

    internal static async Task WriteMirror(string path, DockerHubMirrorEnum mirror)
    {
        var root = File.Exists(path)
            ? System.Text.Json.Nodes.JsonNode.Parse(await File.ReadAllTextAsync(path)) as System.Text.Json.Nodes.JsonObject
            : new System.Text.Json.Nodes.JsonObject();
        if (root is null) throw new InvalidDataException("Docker 配置必须是 JSON 对象。");
        if (mirror == DockerHubMirrorEnum.Default) root.Remove(MirrorFieldName);
        else root[MirrorFieldName] = new System.Text.Json.Nodes.JsonArray(mirror.ToStringFast(useMetadataAttributes: true));
        await Create.WriteConfig(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }
}
