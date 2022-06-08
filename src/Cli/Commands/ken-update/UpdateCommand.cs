using System;
using System.CommandLine;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using Cli.Utils;
using kentxxq.Utils;
using Octokit;
using Spectre.Console;
using FileMode = System.IO.FileMode;

namespace Cli.Commands.ken_update;

public static class UpdateCommand
{
    /// <summary>
    /// 国内的下载地址
    /// </summary>
    private const string DownloadServer = @"http://tools.kentxxq.com/";

    /// <summary>
    /// 服务器上的文件名称
    /// </summary>
    private static readonly string ServerFileName = GetServerFileName();

    /// <summary>
    /// 具体文件下载地址
    /// </summary>
    private static readonly string DownloadUrl = DownloadServer + ServerFileName;

    /// <summary>
    /// 新版本程序下载后的地址
    /// </summary>
    private static readonly string NewFilePath = Path.Combine(Directory.GetCurrentDirectory(), ServerFileName + "new");

    /// <summary>
    /// 程序在当前平台上的命名
    /// </summary>
    private static readonly string FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ken.exe" : "ken";

    /// <summary>
    /// 正在执行的程序路径
    /// </summary>
    private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);


    public static Command GetCommand()
    {
        var command = new Command("update", "update ken command");
        command.SetHandler(context => { Run(); });
        return command;
    }

    private static void Run()
    {
        // 检查当前版本
        var version = Assembly.GetAssemblyInformationalVersion();
        MyAnsiConsole.MarkupSuccessLine($"current version:{version}");
        // 检查github上面的版本
        var client = new GitHubClient(new ProductHeaderValue("ken-cli"));
        var latestRelease = client.Repository.Release.GetLatest("kentxxq", "kentxxq.Cli").Result;
        var latestVersion = latestRelease.TagName;
        MyAnsiConsole.MarkupSuccessLine($"latest version:{latestVersion}");
        if (latestVersion == version)
        {
            MyAnsiConsole.MarkupSuccessLine("It's the latest version now!");
        }
        else
        {
            // 下载对应最新的cli
            AnsiConsole.Status()
                .Start("Downloading...", ctx => { DownloadNewVersion(); });

            // 移动当前的版本，将新版本cli放到现有的位置
            File.Move(FilePath, FilePath + "old");
            File.Move(NewFilePath, FilePath);
        }
    }

    private static void DownloadNewVersion()
    {
        var httpClient = new HttpClient();
        if (File.Exists(NewFilePath)) File.Delete(NewFilePath);
        using var fs = new FileStream(NewFilePath, FileMode.Create, FileAccess.Write);
        httpClient.GetAsync(DownloadUrl).Result.Content.CopyTo(fs, null, CancellationToken.None);
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