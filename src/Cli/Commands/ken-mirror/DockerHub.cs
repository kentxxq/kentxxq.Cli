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
    private static readonly Option<DockerHubMirrorEnum> DockerHubMirror = new(
        new[] { "-m", "--mirror" },
        ()=>DockerHubMirrorEnum.NetEase163,
        $"default {DockerHubMirrorEnum.NetEase163} registry: https://1ocw3lst.mirror.aliyuncs.com"
    );

    private static readonly string ConfigPath =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),@".docker\daemon.json") : "/etc/docker/daemon.json";

    private const string MirrorFieldName = "registry-mirrors";
    
    public static Command GetCommand()
    {
        var command = new Command("dockerhub", "set dockerhub mirror registry")
        {
            DockerHubMirror
        };
        command.SetHandler(async context =>
        {
            var dockerHubMirror = context.ParseResult.GetValueForOption(DockerHubMirror);
            MyAnsiConsole.MarkupSuccessLine($"使用的dockerHub镜像为 :{dockerHubMirror}");
            await SetDockerhubMirror(dockerHubMirror);
        });
        return command;
    }

    private static async Task SetDockerhubMirror(DockerHubMirrorEnum dockerHubMirrorEnum)
    {
        var mirrorUrl = dockerHubMirrorEnum.ToStringFast();

        // 没有配置就创建一个
        if (!File.Exists(ConfigPath))
        {
            await Create.CreateFile(ConfigPath,"{}");
        }

        // 拿到现有配置
        var configSteam = File.Open(ConfigPath,FileMode.Open,FileAccess.ReadWrite);
        using var document = await JsonDocument.ParseAsync(configSteam);
        configSteam.Close();
        
        // 新对象保存数据
        using var stream = new MemoryStream();
        await using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject(); // 开始写入对象
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Name != MirrorFieldName)
            {
                property.WriteTo(writer); // 将原有的属性复制到新对象中
            }
        }
        
        // DockerHubMirrorEnum.Default是空.代表不使用镜像
        if (dockerHubMirrorEnum != DockerHubMirrorEnum.Default)
        {
            writer.WritePropertyName(MirrorFieldName);
            writer.WriteStartArray();
            writer.WriteStringValue(mirrorUrl); // 添加一个新属性
            writer.WriteEndArray();
            writer.WriteEndObject(); // 结束对象
        }
        
        await writer.FlushAsync();
        var updatedJsonContent = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        // 将更新后的 JSON 字符串写回到原始文件中
        await File.WriteAllTextAsync(ConfigPath, updatedJsonContent);
        MyAnsiConsole.MarkupSuccessLine($"配置成功,验证查看: {ConfigPath}");
        MyAnsiConsole.MarkupWarningLine("你应该重启docker生效");
        
    }
}