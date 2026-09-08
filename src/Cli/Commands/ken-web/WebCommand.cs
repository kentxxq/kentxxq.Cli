using System.CommandLine;
using System.IO;
using Cli.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Cli.Commands.ken_web;

public static class WebCommand
{
    /// <summary>
    /// web的根路径
    /// </summary>
    private static readonly Option<string> Webroot = new("--webroot", "-w") { Description = "file path", DefaultValueFactory = _ => "." };

    /// <summary>
    /// http-server的端口
    /// </summary>
    private static readonly Option<int> Port = new("--port", "-p") { Description = "http port", DefaultValueFactory = _ => 5000 };

    public static Command GetCommand()
    {
        var command = new Command("web", "static file http server")
        {
            Webroot,
            Port
        };
        command.SetAction(async (context, cancellationToken) =>
        {
            var webroot = context.GetValue(Webroot);
            var port = context.GetValue(Port);

            if (string.IsNullOrEmpty(webroot))
            {
                webroot = Directory.GetCurrentDirectory();
            }
            else if (!Path.IsPathRooted(webroot))
            {
                webroot = Path.Combine(Directory.GetCurrentDirectory(), webroot!);
            }

            if (port is < 1 or > 65535) throw new ArgumentException("端口无效。");
            await Run(webroot, port, cancellationToken);
        });
        return command;
    }

    private static async Task Run(string webroot, int port, CancellationToken ct)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDirectoryBrowser();
        // 过滤掉内置的日志
        builder.Logging.AddFilter((provider, category, logLevel) =>
        {
            if (category is null)
            {
                return false;
            }

            return !category.StartsWith("Microsoft");
        });

        await using var app = builder.Build();
        using var fileProvider = new PhysicalFileProvider(webroot);
        // 记录日志
        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.StatusCode.ToString().StartsWith("2"))
            {
                MyAnsiConsole.MarkupSuccessLine(
                    $"{context.Request.Protocol} {context.Request.Method} fake_url {context.Response.StatusCode} {context.Response.ContentType} {context.Response.ContentLength}");
            }
            else
            {
                MyAnsiConsole.MarkupWarningLine(
                    $"{context.Request.Protocol} {context.Request.Method} fake_url {context.Response.StatusCode} {context.Response.ContentType} {context.Response.ContentLength}");
            }
        });
        // 静态文件
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider
        });
        // 文件浏览
        app.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = fileProvider
        });

        app.Urls.Add($"http://*:{port}");
        await app.StartAsync(ct);
        MyAnsiConsole.MarkupSuccessLine($"监听 http://0.0.0.0:{port}，目录浏览已启用。");
        await app.WaitForShutdownAsync(ct);
    }
}
