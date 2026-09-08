using System.CommandLine;
using System.Reflection;
using System.Runtime.InteropServices;
using Cli.Commands.ken_update.Proxy;
using Cli.Utils;
using Octokit;
using FileMode = System.IO.FileMode;

namespace Cli.Commands.ken_update;

public static class UpdateCommand
{
    private const string DownloadServer = "https://github.com/kentxxq/kentxxq.Cli/releases/download/";
    private static readonly Option<bool> Force = new("--force", "-f") { Description = "强制更新" };
    private static readonly Option<string?> Version = new("--ken-version", "-kv") { Description = "指定版本标签" };
    private static readonly Option<ProxyEnum> Proxy = new("--proxy", "-p") { DefaultValueFactory = _ => ProxyEnum.Github, Description = "下载代理，默认 GitHub 直连" };
    private static readonly Option<string> Token = new("--token", "-t") { DefaultValueFactory = _ => "", Description = "GitHub API 令牌" };

    public static Command GetCommand()
    {
        var command = new Command("update", "更新当前 ken 程序") { Force, Version, Proxy, Token };
        command.SetAction(async (result, ct) =>
        {
            if (!Enum.IsDefined(result.GetValue(Proxy))) throw new ArgumentException("代理选项无效。");
            var file = Environment.ProcessPath ?? throw new InvalidOperationException("无法定位当前程序。");
            if (Path.GetFileNameWithoutExtension(file).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("请使用发布后的独立程序执行更新。");
            var asset = GetServerFileName();
            var current = ThisAssembly.Info.InformationalVersion.Split('+')[0];
            MyAnsiConsole.MarkupSuccessLine($"当前版本：{current}");
            var version = result.GetValue(Version);
            if (string.IsNullOrWhiteSpace(version))
            {
                var client = new GitHubClient(new ProductHeaderValue("ken-cli"));
                var token = result.GetValue(Token);
                if (!string.IsNullOrEmpty(token)) client.Credentials = new Credentials(token);
                var release = await client.Repository.Release.GetLatest("kentxxq", "kentxxq.Cli").WaitAsync(ct);
                version = release.TagName;
            }
            if (!result.GetValue(Force) && current.TrimStart('v') == version.TrimStart('v'))
            {
                MyAnsiConsole.MarkupSuccessLine("已经是最新版本。");
                return;
            }
            var proxy = result.GetValue(Proxy);
            var url = new ProxyStrategy(proxy).GetDownloadUrl(proxy, DownloadServer + Uri.EscapeDataString(version) + "/" + asset);
            using var clientHttp = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            var temporary = file + ".ken-download-" + Guid.NewGuid().ToString("N");
            try
            {
                await Download(clientHttp, new Uri(url), temporary, ct);
                if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(temporary, File.GetUnixFileMode(file));
                ReplaceExecutable(file, temporary);
                MyAnsiConsole.MarkupSuccessLine("更新成功，旧程序已保留为 .ken-old。");
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        });
        return command;
    }

    internal static async Task Download(HttpClient client, Uri url, string destination, CancellationToken ct)
    {
        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
            await using (var stream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
            {
                await response.Content.CopyToAsync(stream, ct);
                if (stream.Length < 4 || response.Content.Headers.ContentLength is long expected && stream.Length != expected)
                    throw new InvalidDataException("下载文件为空或不完整。");
            }
            // 拒绝返回 HTTP 200 的错误网页；此检查不替代发布方签名。
            await using var input = File.OpenRead(destination);
            var header = new byte[4];
            await input.ReadExactlyAsync(header, ct);
            var valid = OperatingSystem.IsWindows() ? header[0] == 'M' && header[1] == 'Z'
                : OperatingSystem.IsLinux() ? header.SequenceEqual(new byte[] { 0x7f, 0x45, 0x4c, 0x46 })
                : header.SequenceEqual(new byte[] { 0xcf, 0xfa, 0xed, 0xfe }) || header.SequenceEqual(new byte[] { 0xfe, 0xed, 0xfa, 0xcf });
            if (!valid) throw new InvalidDataException("下载内容不是当前平台的可执行文件。");
        }
        catch
        {
            if (File.Exists(destination)) File.Delete(destination);
            throw;
        }
    }

    internal static void ReplaceExecutable(string current, string downloaded)
    {
        if (!File.Exists(downloaded)) throw new FileNotFoundException("更新文件不存在。");
        var backup = current + ".ken-old";
        File.Move(current, backup, true);
        try { File.Move(downloaded, current); }
        catch
        {
            File.Move(backup, current);
            throw;
        }
    }

    private static string GetServerFileName()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var rid = (OperatingSystem.IsWindows(), OperatingSystem.IsLinux(), OperatingSystem.IsMacOS(), architecture) switch
        {
            (true, _, _, Architecture.X64) => "win-x64.exe",
            (true, _, _, Architecture.X86) => "win-x86.exe",
            (true, _, _, Architecture.Arm64) => "win-arm64.exe",
            (_, true, _, Architecture.X64) => "linux-x64",
            (_, true, _, Architecture.Arm) => "linux-arm",
            (_, true, _, Architecture.Arm64) => "linux-arm64",
            (_, _, true, Architecture.X64) => "osx-x64",
            (_, _, true, Architecture.Arm64) => "osx-arm64",
            _ => throw new PlatformNotSupportedException("当前平台没有发布产物。")
        };
        return "ken-" + rid;
    }
}
