using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cli.Extensions;
using Cli.Utils;
using kentxxq.Extensions.String;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Cli.Commands.ken_sp
{
    public class SocketPingCommand
    {
        private static readonly Argument<string> url = new("url", "url: kentxxq.com:443");

        private static readonly Option<int> retryTimes = new(new[] { "-n", "--retryTimes" }, () => 0, "default:0,retry forever");

        private static readonly Option<int> timeout = new(new[] { "-t", "--timeout" }, () => 2, "default:2 seconds");

        private static readonly Option<bool> quit = new(new[] { "-q", "--quit" }, () => false, "Quit after connection succeeded");


        public static Command GetCommand()
        {
            var command = new Command("sp", "socketping") {
                url,
                retryTimes,
                timeout,
                quit
            };

            command.SetHandler<string, int, int, bool, CancellationToken>(Run, url, retryTimes, timeout, quit);
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
                    return;
                }
                return;
            }
            else
            {
                for (var i = 0; i < retryTimes; i++)
                {
                    result = Connect(ipEndPoint, timeout, ct);
                    if ((result && quit) || ct.IsCancellationRequested)
                    {
                        return;
                    }
                }
                return;
            }

        }

        private static bool Connect(IPEndPoint ipEndPoint, int timeout, CancellationToken token)
        {
            using var tcp = new TcpClient();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                tcp.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port).Wait(timeout * 1000, token);
                stopwatch.Stop();

                if (tcp.Connected)
                {
                    AnsiConsole.MarkupLine($"request [green]successed[/]. waited { stopwatch.ElapsedMilliseconds.NetworkDelayWithColor()} ms");
                }
                else
                {
                    AnsiConsole.MarkupLine($"request [red]failed[/]. waited { stopwatch.ElapsedMilliseconds.NetworkDelayWithColor()} ms");
                }
            }
            catch (OperationCanceledException)
            {
                MyAnsiConsole.MarkupErrorLine("操作取消");
            }
            catch (Exception e)
            {
                MyAnsiConsole.MarkupErrorLine($"连接失败:{e.Message}");
            }

            return tcp.Connected;
        }

    }
}