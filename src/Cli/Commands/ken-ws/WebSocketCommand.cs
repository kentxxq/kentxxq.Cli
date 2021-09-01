using Cli.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
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

            command.Handler = CommandHandler.Create<Uri, CancellationToken, IHost>(Run);
            return command;
        }

        private static async Task Run(Uri wsUrl, CancellationToken ct, IHost host)
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
            var render = host.Services.GetService<ConsoleRenderer>();
            var ws = new ClientWebSocket();
            //ws.Options.RemoteCertificateValidationCallback = delegate { return true; };
            try
            {
                await ws.ConnectAsync(wsUrl, ct);
            }
            catch (Exception e)
            {
                render.RenderToRegion($"连接失败:{e.Message}".Color(ForegroundColorSpan.Red()), Region.EntireTerminal);
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
                render.RenderSuccess($"<< {text}");
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }
}
