using System;
using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cli.Extensions;
using Cli.Utils;
using kentxxq.Extensions.String;
using Spectre.Console;

namespace Cli.Commands.ken_sp;

public static class SocketPingCommand
{
    private static readonly Argument<string> Url = new("url", "url: kentxxq.com:443");

    private static readonly Option<int> RetryTimes = new(new[] { "-n", "--retryTimes" }, () => 0,
        "default:0,retry forever");

    private static readonly Option<int> Timeout = new(new[] { "-t", "--timeout" }, () => 2, "default:2 seconds");

    private static readonly Option<bool> Quit = new(new[] { "-q", "--quit" }, () => false,
        "Quit after connection succeeded");


    public static Command GetCommand()
    {
        var command = new Command("sp", "socket ping")
        {
            Url,
            RetryTimes,
            Timeout,
            Quit
        };

        command.SetHandler(context =>
        {
            var url = context.ParseResult.GetValueForArgument(Url);
            var retryTimes = context.ParseResult.GetValueForOption(RetryTimes);
            var timeout = context.ParseResult.GetValueForOption(Timeout);
            var quit = context.ParseResult.GetValueForOption(Quit);
            var ct = context.GetCancellationToken();
            Run(url, retryTimes, timeout, quit, ct);
        });
        return command;
    }


    private static void Run(string url, int retryTimes, int timeout, bool quit, CancellationToken ct)
    {
        bool result;

        IPEndPoint ipEndPoint;
        try
        {
            ipEndPoint = url.UrlToIPEndPoint();
        }
        catch (Exception e)
        {
            MyAnsiConsole.MarkupErrorLine($"parse error:{e.Message}");
            return;
        }


        if (retryTimes == 0)
        {
            while (!ct.IsCancellationRequested)
            {
                result = Connect(ipEndPoint, timeout, ct);
                Thread.Sleep(500);
                if (result && quit) return;
            }

            return;
        }

        for (var i = 0; i < retryTimes; i++)
        {
            result = Connect(ipEndPoint, timeout, ct);
            if ((result && quit) || ct.IsCancellationRequested) return;
        }
    }

    private static bool Connect(IPEndPoint ipEndPoint, int timeout, CancellationToken token)
    {
        using var tcp = new TcpClient();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            // ReSharper disable once MethodSupportsCancellation
            tcp.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port).Wait(timeout * 1000, token);
            stopwatch.Stop();

            AnsiConsole.MarkupLine(
                tcp.Connected
                    ? $"request [green]successes[/]. waited {stopwatch.ElapsedMilliseconds.NetworkDelayWithColor()} ms"
                    : $"request [red]failed[/]. waited {stopwatch.ElapsedMilliseconds.NetworkDelayWithColor()} ms");
        }
        catch (OperationCanceledException)
        {
            MyAnsiConsole.MarkupErrorLine("operation canceled");
        }
        catch (Exception e)
        {
            MyAnsiConsole.MarkupErrorLine($"connect failed:{e.Message}");
        }

        return tcp.Connected;
    }
}