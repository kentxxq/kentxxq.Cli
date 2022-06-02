using System;
using System.CommandLine;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using Cli.Utils;
using kentxxq.Utils;
using Octokit;
using FileMode = System.IO.FileMode;

namespace Cli.Commands.ken_update;

public static class UpdateCommand
{
    private const string DownloadServer = @"http://tool.kentxxq.com/";
    private static readonly string FileName = GetFileName();
    private static readonly string NewFilePath = Path.Combine(Directory.GetCurrentDirectory(), FileName + "new");
    private static readonly string DownloadUrl = DownloadServer + FileName;
    
    public static Command GetCommand()
    {
        var command = new Command("update", "update ken command");
        command.SetHandler(Run);
        return command;
    }

    private static void Run()
    {
        DownloadNewVersion();

        // // 检查当前版本
        // var version = Assembly.GetAssemblyInformationalVersion();
        // // 检查github上面的版本
        // var client = new GitHubClient(new ProductHeaderValue("ken-cli"));
        // var latestRelease = client.Repository.Release.GetLatest("kentxxq", "kentxxq.Cli").Result;
        // var info = client.GetLastApiInfo();
        // var latestVersion = latestRelease.TagName;
        // if (latestVersion == version)
        // {
        //     MyAnsiConsole.MarkupSuccessLine("It's the latest version now!");
        // }
        // else
        // {
        //     // 下载对应最新的cli
        //     
        //     // 移动当前的版本，将新版本cli放到现有的位置
        // }
    }

    private static void DownloadNewVersion()
    {
        var httpClient = new HttpClient();
        using var fs = new FileStream(NewFilePath, FileMode.CreateNew, FileAccess.Write);
        httpClient.GetAsync(DownloadUrl).Result.Content.CopyTo(fs,null,CancellationToken.None);
    }

    private static string GetFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("linux");
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
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("windows");
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
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("osx");
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
        }

        throw new ArgumentException("unknown os platform");
    }
}