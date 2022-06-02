using System.CommandLine;
using System.IO;
using Cli.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Cli.Commands.ken_web;

public static class WebCommand
{
    private static readonly Option<string> Webroot = new(new[] { "-w", "--webroot" }, () => ".", "file path");
    private static readonly Option<int> Port = new(new[] { "-p", "--port" }, () => 5000, "http port");

    public static Command GetCommand()
    {
        var command = new Command("web", "static file http server")
        {
            Webroot,
            Port
        };
        command.SetHandler<string, int>(Run, Webroot, Port);
        return command;
    }

    private static void Run(string webroot, int port)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDirectoryBrowser();
        builder.Logging.AddFilter((provider, category, logLevel) => !category.StartsWith("Microsoft"));

        if (webroot.IsNullOrEmpty())
            webroot = builder.Environment.WebRootPath;
        else if (!Path.IsPathRooted(webroot)) webroot = Path.Combine(Directory.GetCurrentDirectory(), webroot);

        var app = builder.Build();
        var fileProvider = new PhysicalFileProvider(webroot);
        // 记录日志
        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.StatusCode.ToString().StartsWith("2"))
                MyAnsiConsole.MarkupSuccessLine(
                    $"{context.Request.Protocol} {context.Request.Method} {context.Request.Path} {context.Response.StatusCode} {context.Response.ContentType} {context.Response.ContentLength}");
            else
                MyAnsiConsole.MarkupWarningLine(
                    $"{context.Request.Protocol} {context.Request.Method} {context.Request.Path} {context.Response.StatusCode} {context.Response.ContentType} {context.Response.ContentLength}");
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

#if DEBUG
        MyAnsiConsole.MarkupSuccessLine($"listening http://localhost:{port}");
#else
        MyAnsiConsole.MarkupSuccessLine($"listening http://0.0.0.0:{port}");
#endif
        app.Run($@"http://*:{port}");
    }
}