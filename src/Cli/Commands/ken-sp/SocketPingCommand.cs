using Cli.Extensions;
using kentxxq.Extensions.String;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Cli.Commands.ken_sp
{
    public class SocketPingCommand
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

            command.Handler = CommandHandler.Create<string, SocketPingOptions, CancellationToken, IHost>(Run);
            return command;
        }


        private static int Run(string url, SocketPingOptions socketPingOptions, CancellationToken ct, IHost host)
        {
            IPEndPoint ipEndPoint = null!;
            bool result;
            var render = host.Services.GetService<ConsoleRenderer>();

            try
            {
                ipEndPoint = url.UrlToIPEndPoint();
            }
            catch (Exception e)
            {
                render.RenderError($"parse error:{e.Message}");
                return 1;
            }


            if (socketPingOptions.RetryTimes == 0)
            {
                while (!ct.IsCancellationRequested)
                {
                    result = Connect(ipEndPoint, socketPingOptions.Timeout, render, ct);
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
                    result = Connect(ipEndPoint, socketPingOptions.Timeout, render, ct);
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

        private static bool Connect(IPEndPoint ipEndPoint, int timeout, ConsoleRenderer render, CancellationToken token)
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
                    render.RenderToRegion($"request { "successed".Color(ForegroundColorSpan.Green())}. waited { stopwatch.ElapsedMilliseconds} ms", Region.EntireTerminal);
                }
                else
                {
                    render.RenderToRegion($"request { "failed".Color(ForegroundColorSpan.Red())}. waited { stopwatch.ElapsedMilliseconds} ms", Region.EntireTerminal);
                }
                Console.WriteLine("");
            }
            catch (OperationCanceledException)
            {
                render.RenderError("操作取消");
            }
            catch (Exception e)
            {
                render.RenderError($"连接失败:{e.Message}");
            }

            return tcp.Connected;
        }

    }
}