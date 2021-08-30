using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cli.Extensions;
using kentxxq.Extensions.String;

namespace Cli.Commands.ken_sp
{
    internal class SocketPingCommand
    {
        public static Command GetCommand()
        {
            var command = new Command("sp") {
                new Argument<string>("url",
                                     "url: kentxxq.com:443"),
                new Option<int>(new[]{"-n", "--retryTimes"},
                                ()=>0,
                                "default:0,retry forever"),
                new Option<int>(new[] { "-t", "--timeout" },
                                () => 2,
                                "default:2 seconds"),
                new Option<bool>(new[] { "-q", "--quit" },
                                 () => false,
                                 "Quit after connection succeeded")
            };

            command.Handler = CommandHandler.Create<string, SocketPingOptions, CancellationToken>(Run);
            return command;
        }


        private static int Run(string url, SocketPingOptions socketPingOptions, CancellationToken ct)
        {
            IPEndPoint ipEndPoint = null!;
            bool result;
            var console = new SystemConsole();
            var consoleRender = new ConsoleRenderer(console, OutputMode.Ansi, true);

            try
            {
                ipEndPoint = url.UrlToIPEndPoint();
            }
            catch (Exception e)
            {
                Console.WriteLine($"parse error:{e.Message}");
                return 1;
            }


            if (socketPingOptions.RetryTimes == 0)
            {
                while (!ct.IsCancellationRequested)
                {
                    result = Connect(ipEndPoint, socketPingOptions.Timeout, consoleRender, ct);
                    if (result && socketPingOptions.Quit)
                    {
                        return 0;
                    }
                }
                return 1;
            }
            else
            {
                for (var i = 0; i < socketPingOptions.RetryTimes; i++)
                {
                    result = Connect(ipEndPoint, socketPingOptions.Timeout, consoleRender, ct);
                    if (result && socketPingOptions.Quit)
                    {
                        return 0;
                    }
                    else if (ct.IsCancellationRequested)
                    {
                        return 1;
                    }
                }
                return 1;
            }

        }

        private static bool Connect(IPEndPoint ipEndPoint, int timeout, ConsoleRenderer consoleRender, CancellationToken token)
        {
            var region = Region.EntireTerminal;
            using var tcp = new TcpClient();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                tcp.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port).Wait(timeout * 1000, token);
                stopwatch.Stop();

                if (tcp.Connected)
                {
                    consoleRender.RenderToRegion($"request { "successed".Color(ForegroundColorSpan.Green())}. waited { stopwatch.ElapsedMilliseconds} ms", region);
                }
                else
                {
                    consoleRender.RenderToRegion($"request { "failed".Color(ForegroundColorSpan.Red())}. waited { stopwatch.ElapsedMilliseconds} ms", region);
                }
                Console.WriteLine("");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("操作取消");
            }
            catch (Exception e)
            {
                Console.WriteLine($"连接失败:{e.Message}");
            }

            return tcp.Connected;
        }

    }
}