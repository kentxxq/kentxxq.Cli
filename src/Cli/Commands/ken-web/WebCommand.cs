﻿using System.CommandLine;
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
    /// <summary>
    /// web的根路径
    /// </summary>
    private static readonly Option<string> Webroot = new(new[] { "-w", "--webroot" }, () => ".", "file path");

    /// <summary>
    /// http-server的端口
    /// </summary>
    private static readonly Option<int> Port = new(new[] { "-p", "--port" }, () => 5000, "http port");

    public static Command GetCommand()
    {
        var command = new Command("web", "static file http server")
        {
            Webroot,
            Port
        };
        command.SetHandler(context =>
        {
            var webroot = context.ParseResult.GetValueForOption(Webroot);
            var port = context.ParseResult.GetValueForOption(Port);

            if (string.IsNullOrEmpty(webroot))
            {
                webroot = Directory.GetCurrentDirectory();
            }
            else if (!Path.IsPathRooted(webroot))
            {
                webroot = Path.Combine(Directory.GetCurrentDirectory(), webroot!);
            }

            Run(webroot, port);
        });
        return command;
    }

    private static void Run(string webroot, int port)
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

        var app = builder.Build();
        var fileProvider = new PhysicalFileProvider(webroot);
        // 记录日志
        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.StatusCode.ToString().StartsWith("2"))
            {
                MyAnsiConsole.MarkupSuccessLine(
                    $"{context.Request.Protocol} {context.Request.Method} {context.Request.Path} {context.Response.StatusCode} {context.Response.ContentType} {context.Response.ContentLength}");
            }
            else
            {
                MyAnsiConsole.MarkupWarningLine(
                    $"{context.Request.Protocol} {context.Request.Method} {context.Request.Path} {context.Response.StatusCode} {context.Response.ContentType} {context.Response.ContentLength}");
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

#if DEBUG
        MyAnsiConsole.MarkupSuccessLine($"listening http://localhost:{port}");
#else
        MyAnsiConsole.MarkupSuccessLine($"listening http://0.0.0.0:{port},http://127.0.0.1:{port}");
#endif
        app.Urls.Add($"http://*:{port}");
        app.Run();
    }
}
