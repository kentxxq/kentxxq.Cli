using System;
using System.CommandLine;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cli.Commands.ken_update.Proxy;
using Cli.Utils;
using Cli.Utils.Ip;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using Spectre.Console;
using FileMode = System.IO.FileMode;

namespace Cli.Commands.ken_update;

public static class UpdateCommand
{
    /// <summary>
    /// 下载地址
    /// </summary>
    private const string DownloadServer = @"https://github.com/kentxxq/kentxxq.Cli/releases/download/";

    /// <summary>
    /// 服务器上的文件名称
    /// </summary>
    private static readonly string ServerFileName = GetServerFileName();

    /// <summary>
    /// 新版本程序下载后的地址
    /// </summary>
    private static readonly string NewFilePath = Path.Combine(AppContext.BaseDirectory, ServerFileName + "new");

    /// <summary>
    /// 老版本程序的备份地址
    /// </summary>
    private static readonly string OldFilePath = Path.Combine(AppContext.BaseDirectory, ServerFileName + "old");

    /// <summary>
    /// 程序在当前平台上的命名
    /// </summary>
    private static readonly string FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ken.exe" : "ken";

    /// <summary>
    /// 正在执行的程序路径
    /// </summary>
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, FileName);

    /// <summary>
    /// 当前程序的版本号
    /// </summary>
    private static readonly string CurrentVersion = ThisAssembly.Info.InformationalVersion;

    /// <summary>
    /// 强制升级
    /// </summary>
    private static readonly Option<bool> Force = new(new[] { "-f", "--force" }, () => false,
        "force update current version");

    /// <summary>
    /// 指定版本号
    /// </summary>
    private static readonly Option<string> Version = new(new[] { "-kv", "--ken-version" },
        "force upgrade to specific current version");

    /// <summary>
    /// 在中国就启用代理地址
    /// </summary>
    private static readonly Option<ProxyEnum> Proxy = new(new[] { "-p", "--proxy" }, () => IpService.ImInChina().Result ? ProxyEnum.Ghproxy : ProxyEnum.Github,
        "use proxy");

    /// <summary>
    /// 请求github-api的token，默认每小时60次请求限制
    /// </summary>
    private static readonly Option<string> Token = new(new[] { "-t", "--token" }, () => "",
        "github token for query github-api");


    public static Command GetCommand()
    {
        var command = new Command("update", "update ken command")
        {
            Force,
            Version,
            Proxy,
            Token
        };

        command.SetHandler(async context =>
        {
            var force = context.ParseResult.GetValueForOption(Force);
            var specificVersion = context.ParseResult.GetValueForOption(Version);
            var proxy = context.ParseResult.GetValueForOption(Proxy);
            var token = context.ParseResult.GetValueForOption(Token);
            // 需要下载的版本号
            var downloadVersion = specificVersion;
            // 打印当前信息
            PrintCurrentInformation();
            // 如果没有指定版本，则拿到最新版本号
            if (specificVersion.IsNullOrEmpty())
            {
                try
                {
                    downloadVersion = await GetLatestVersion(token!);
                }
                catch (RateLimitExceededException e)
                {
                    MyAnsiConsole.MarkupErrorLine($"{e.Message}. you can set token through -t");
                    context.ExitCode = 1;
                    return;
                }

                AnsiConsole.MarkupLine($"latest version:{downloadVersion}");
            }

            // 判断是否更新程序
            if (!force && CurrentVersion == downloadVersion)
                MyAnsiConsole.MarkupSuccessLine("It's the latest version now!");
            else
            {
                MyAnsiConsole.MarkupSuccessLine($"using {proxy.ToString()}");
                await UpdateKen(downloadVersion!, proxy);
            }
        });
        return command;
    }

    /// <summary>
    /// 输出当前的程序信息
    /// </summary>
    private static void PrintCurrentInformation()
    {
        var path = new TextPath($"{FilePath}")
            .RootStyle(new Style(Color.Red))
            .SeparatorStyle(new Style(Color.Green))
            .StemStyle(new Style(Color.Blue))
            .LeafStyle(new Style(Color.Yellow));
        Console.Write("current file: ");
        AnsiConsole.Write(path);
        AnsiConsole.MarkupLine($"current version: {CurrentVersion}");
    }

    /// <summary>
    /// 获取github上最新的版本号
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async Task<string> GetLatestVersion(string token)
    {
        var client = new GitHubClient(new ProductHeaderValue("ken-cli"));
        if (!token.IsNullOrEmpty()) client.Credentials = new Credentials(token);

        var latestRelease = await client.Repository.Release.GetLatest("kentxxq", "kentxxq.Cli");
        return latestRelease.TagName;
    }

    /// <summary>
    /// 更新本体
    /// </summary>
    private static async Task UpdateKen(string version, ProxyEnum proxy)
    {
        // 下载对应最新的cli
        await AnsiConsole.Status()
            .StartAsync("Downloading...", async _ =>
            {
                var ok = await DownloadNewVersion(version, proxy);
                if (ok)
                {
                    if (File.Exists(OldFilePath)) File.Delete(OldFilePath);
                    // 移动当前的版本
                    File.Move(FilePath, OldFilePath);
                    // 将新版本cli放到现有的位置
                    File.Move(NewFilePath, FilePath);

                    // 非windows系统提示执行+x
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        MyAnsiConsole.MarkupWarningLine($"you should : chmod +x {FilePath}");

                    MyAnsiConsole.MarkupSuccessLine("update successfully");
                }
            });
    }

    /// <summary>
    /// 下载最新的版本
    /// </summary>
    /// <param name="version">特定的版本号</param>
    /// <param name="proxy">是否启用代理</param>
    private static async Task<bool> DownloadNewVersion(string version, ProxyEnum proxy)
    {
        var httpClient = new HttpClient();
        if (File.Exists(NewFilePath)) File.Delete(NewFilePath);
        await using var fs = new FileStream(NewFilePath, FileMode.Create, FileAccess.Write);

        var p = new ProxyStrategy(proxy);
        var url = p.GetDownloadUrl(proxy, DownloadServer + version + "/" + ServerFileName);

        var res = await httpClient.GetAsync(url);
        if (res.IsSuccessStatusCode)
        {
            await res.Content.CopyToAsync(fs);
            return true;
        }

        // Activator.CreateInstance(
        //     Type.GetType("System.EventArgs;System.Random") ?? throw new InvalidOperationException());

        MyAnsiConsole.MarkupErrorLine($"{version} not found!!!");
        return false;
    }

    /// <summary>
    /// 根据不同平台，拿到服务器上对应的文件名。例如ken-win-x64.exe
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static string GetServerFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            switch (RuntimeInformation.OSArchitecture)
            {
                // TODO 还少了musl
                case Architecture.Arm:
                    return "ken-linux-arm";
                case Architecture.Arm64:
                    return "ken-linux-arm64";
                case Architecture.X64:
                    return "ken-linux-x64";
                case Architecture.X86:
                case Architecture.Wasm:
                case Architecture.S390x:
                default:
                    throw new ArgumentException("unsupported os platform");
            }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.Arm:
                    return "ken-win-arm.exe";
                case Architecture.Arm64:
                    return "ken-win-arm64.exe";
                case Architecture.X86:
                    return "ken-win-x86.exe";
                case Architecture.X64:
                    return "ken-win-x64.exe";
                case Architecture.Wasm:
                case Architecture.S390x:
                default:
                    throw new ArgumentException("unsupported os platform");
            }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X64:
                    return "ken-osx-x64";
                case Architecture.Arm64:
                    return "ken-osx-arm64";
                case Architecture.X86:
                case Architecture.Wasm:
                case Architecture.S390x:
                case Architecture.Arm:
                default:
                    throw new ArgumentException("unsupported os platform");
            }

        throw new ArgumentException("unknown os platform");
    }
}