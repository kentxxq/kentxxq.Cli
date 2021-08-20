using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Cli.Extensions;
using kentxxq.Extensions.String;

namespace Cli.Commands.ken_sp
{
    internal static class SocketPingCommand
    {
        private static readonly Argument<string> Url = new("url", "url: kentxxq.com:443");

        private static readonly Option<int> RetryTimes = new(new[] { "-n", "--retryTimes" }, () => 0, "default:0,retry forever");

        private static readonly Option<int> Timeout = new(new[] { "-t", "--timeout" }, () => 2, "default:2 seconds");

        private static readonly Option<bool> Quit = new(new[] { "-q", "--quit" }, () => false, "Quit after connection succeeded");


        public static Command GetCommand()
        {
            var command = new Command("sp");
            command.AddArgument(Url);
            command.AddOption(RetryTimes);
            command.AddOption(Timeout);
            command.AddOption(Quit);

            command.Handler = CommandHandler.Create<string, int, int, bool>(Run);
            return command;
        }


        private static void Run(string url, int retryTimes, int timeout, bool quit)
        {
            var ipEndPoint = url.UrlToIPEndPoint();
            bool result;

            var console = new SystemConsole();
            var consoleRender = new ConsoleRenderer(console, OutputMode.Ansi, true);

            if (retryTimes == 0)
            {
                while (true)
                {
                    result = Connect(ipEndPoint, timeout, consoleRender);
                    if (result && quit)
                    {
                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                for (var i = 0; i < retryTimes; i++)
                {
                    result = Connect(ipEndPoint, timeout, consoleRender);
                    if (result && quit)
                    {
                        Environment.Exit(0);
                    }
                }
                Environment.Exit(1);
            }

        }

        private static bool Connect(IPEndPoint ipEndPoint, int timeout, ConsoleRenderer consoleRender)
        {
            var region = Region.EntireTerminal;

            using var tcp = new TcpClient();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = tcp.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port).Wait(timeout * 1000);
            stopwatch.Stop();

            if (result)
            {
                consoleRender.RenderToRegion($"request { "successed".Color(ForegroundColorSpan.Green())}. waited { stopwatch.ElapsedMilliseconds} ms", region);
            }
            else
            {
                consoleRender.RenderToRegion($"request { "failed".Color(ForegroundColorSpan.Red())}. waited { stopwatch.ElapsedMilliseconds} ms", region);
            }
            Console.WriteLine("");
            return result;
        }

    }
}