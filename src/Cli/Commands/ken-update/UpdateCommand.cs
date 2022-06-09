using System;
using System.CommandLine;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using Cli.Utils;
using kentxxq.Utils;
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
    /// China下载地址
    /// </summary>
    private const string ChinaDownloadServer = @"https://github.abskoop.workers.dev/https://github.com/kentxxq/kentxxq.Cli/releases/download/";
    
    /// <summary>
    /// 服务器上的文件名称
    /// </summary>
    private static readonly string ServerFileName = GetServerFileName();
    
    /// <summary>
    /// 新版本程序下载后的地址
    /// </summary>
    private static readonly string NewFilePath = Path.Combine(Directory.GetCurrentDirectory(), ServerFileName + "new");
    
    /// <summary>
    /// 老版本程序的备份地址
    /// </summary>
    private static readonly string OldFilePath = Path.Combine(Directory.GetCurrentDirectory(), ServerFileName + "old");

    /// <summary>
    /// 程序在当前平台上的命名
    /// </summary>
    private static readonly string FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ken.exe" : "ken";

    /// <summary>
    /// 正在执行的程序路径
    /// </summary>
    // private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, FileName);

    private static readonly Option<bool> Force = new(new[] { "-f", "--force" }, () => false,
        "force update current version");
    
    private static readonly Option<string> Version = new(new[] { "-kv", "--ken-version" },
        "force upgrade to specific current version");
    
    private static readonly Option<bool> China = new(new[] { "-cn", "--china" },
        "use proxy url");


    public static Command GetCommand()
    {
        var command = new Command("update", "update ken command")
        {
            Force,
            Version,
            China
        };
        command.SetHandler(Run,Force,Version,China);
        return command;
    }

    private static void Run(bool force,string version,bool cn)
    {
        // 输出基本信息
        MyAnsiConsole.MarkupSuccessLine($"current file: {FilePath}");
#if DEBUG
        MyAnsiConsole.MarkupSuccessLine($"new current file: {NewFilePath}");
        MyAnsiConsole.MarkupSuccessLine($"old current file: {OldFilePath}");
#endif
        // 检查当前版本
        var currentVersion = Assembly.GetAssemblyInformationalVersion();
        MyAnsiConsole.MarkupSuccessLine($"current version:{currentVersion}");

        // 没有指定版本，那就从github获取最新的版本
        if (version.IsNullOrEmpty())
        {
            var client = new GitHubClient(new ProductHeaderValue("ken-cli"));
            var latestRelease = client.Repository.Release.GetLatest("kentxxq", "kentxxq.Cli").Result;
            version = latestRelease.TagName;
            MyAnsiConsole.MarkupSuccessLine($"latest version:{version}");
        }
        
        if (currentVersion == version && !force)
        {
            MyAnsiConsole.MarkupSuccessLine("It's the latest version now!");
        }
        else
        {
            // 下载对应最新的cli
            AnsiConsole.Status()
                .Start("Downloading...", ctx => { DownloadNewVersion(version,cn); });
            
            // 移动当前的版本，将新版本cli放到现有的位置
            if (File.Exists(OldFilePath)) File.Delete(OldFilePath);
            File.Move(FilePath,  OldFilePath);
            File.Move(NewFilePath, FilePath);
            MyAnsiConsole.MarkupSuccessLine("update successfully");
        }
    }

    private static void DownloadNewVersion(string version,bool cn)
    {
        var httpClient = new HttpClient();
        if (File.Exists(NewFilePath)) File.Delete(NewFilePath);
        using var fs = new FileStream(NewFilePath, FileMode.Create, FileAccess.Write);
        string url;
        if (cn)
        {
            url = ChinaDownloadServer + version + "/" + ServerFileName;
        }
        else
        {
            url = DownloadServer + version + "/" + ServerFileName;
        }
        httpClient.GetAsync(url).Result.Content.CopyTo(fs, null, CancellationToken.None);
    }

    private static string GetServerFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            switch (RuntimeInformation.OSArchitecture)
            {
                // TODO 还少了两个类型
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