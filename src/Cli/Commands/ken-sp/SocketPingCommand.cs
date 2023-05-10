using System;
using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cli.Extensions;
using Spectre.Console;
using static System.Threading.Tasks.Task;

namespace Cli.Commands.ken_sp;

public static class SocketPingCommand
{
    /// <summary>
    /// 需要测试的地址
    /// </summary>
    private static readonly Argument<string> Url = new("url", "url: kentxxq.com:443");

    /// <summary>
    /// 重试的次数
    /// </summary>
    private static readonly Option<int> RetryTimes = new(new[] { "-n", "--retryTimes" }, () => 0,
        "default:0,retry forever");

    /// <summary>
    /// 每次连接的超时时间
    /// </summary>
    private static readonly Option<int> Timeout = new(new[] { "-t", "--timeout" }, () => 2, "default:2 seconds");

    /// <summary>
    /// 连接成功后是否退出
    /// </summary>
    private static readonly Option<bool> Quit = new(new[] { "-q", "--quit" }, () => false,
        "Quit after connection succeeded");

    /// <summary>
    /// 存放连接结果
    /// </summary>
    private static bool _result;

    public static Command GetCommand()
    {
        var command = new Command("sp", "socket ping")
        {
            Url,
            RetryTimes,
            Timeout,
            Quit
        };

        command.SetHandler(async context =>
        {
            var url = context.ParseResult.GetValueForArgument(Url);
            var retryTimes = context.ParseResult.GetValueForOption(RetryTimes);
            var timeout = context.ParseResult.GetValueForOption(Timeout);
            var quit = context.ParseResult.GetValueForOption(Quit);
            var ct = context.GetCancellationToken();

            var ipEndPoint = new IPEndPoint((await Dns.GetHostAddressesAsync(url.Split(":")[0], ct))[0], int.Parse(url.Split(":")[1]));
            if (retryTimes == 0)
            {
                while (!ct.IsCancellationRequested)
                {
                    _result = await Connect(ipEndPoint, timeout, ct);
                    Thread.Sleep(500);
                    if (_result && quit)
                    {
                        return;
                    }
                }
            }
            else
            {
                for (var i = 0; i < retryTimes; i++)
                {
                    _result = await Connect(ipEndPoint, timeout, ct);
                    if ((_result && quit) || ct.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        });
        return command;
    }

    private static async Task<bool> Connect(IPEndPoint ipEndPoint, int timeout, CancellationToken token)
    {
        using var tcp = new TcpClient();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // tcp.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port).Wait(timeout * 1000, token);
        var task = tcp.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port, token).AsTask();
        var winner = await WhenAny(
            task, Delay(TimeSpan.FromSeconds(1), token));
        stopwatch.Stop();

        if (winner == task)
        {
            AnsiConsole.MarkupLine(
                $"request [green]successes[/]. waited {stopwatch.ElapsedMilliseconds.NetworkDelayWithColor()} ms");
            return tcp.Connected;
        }

        AnsiConsole.MarkupLine(
            $"request [red]failed[/]. waited {stopwatch.ElapsedMilliseconds.NetworkDelayWithColor()} ms");
        return false;
    }
}