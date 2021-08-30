using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cli.Commands.ken_ws
{
    internal static class WebSocketCommand
    {
        private static readonly Argument<Uri> wsUrl = new("wsUrl", "wsUrl: wss://ws.kentxxq.com/ws");

        public static Command GetCommand()
        {
            var command = new Command("ws");
            command.AddArgument(wsUrl);

            command.Handler = CommandHandler.Create<Uri, CancellationToken>(Run2);
            return command;
        }

        private static async Task Run2(Uri wsUrl, CancellationToken ct)
        {
            //var console = new SystemConsole();
            //var render = new ConsoleRenderer(console, OutputMode.Ansi, true);
            //var region = RegionTools.Live;

            //for (int i = 0; i < 100; i++)
            //{
            //    render.RenderToRegion($"nihao:{i}", region);
            //    Thread.Sleep(50);
            //}

            //var input = System.Console.ReadLine();
            //if (input is not null)
            //{
            //    render.RenderToRegion(input, Region.EntireTerminal);
            //}
            var ws = new ClientWebSocket();
            //ws.Options.RemoteCertificateValidationCallback = delegate { return true; };
            try
            {
                await ws.ConnectAsync(wsUrl, ct);
            }
            catch (Exception e)
            {
                Console.WriteLine($"连接失败:{e.Message}");
                return;
            }

            var buffer = new byte[1024 * 4];
            string input;
            while (!ct.IsCancellationRequested)
            {
                Console.Write(">> ");
                input = Console.ReadLine() ?? "";
                await ws.SendAsync(Encoding.UTF8.GetBytes(input), WebSocketMessageType.Text, true, ct);
                await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                var text = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"<< {text}");
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }
}
