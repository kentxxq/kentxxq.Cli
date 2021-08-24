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

        // private static readonly Option<string> wsUrl2 = new(new[] { "-n", "-nn" }, () => "qwer", "qwer");


        public static Command GetCommand()
        {
            var command = new Command("ws");
            command.AddArgument(wsUrl);
            // command.AddOption(wsUrl2);

            command.Handler = CommandHandler.Create<Uri>(Run2);
            return command;
        }

        private static async Task Run2(Uri wsUrl)
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
            Console.WriteLine($"url: {wsUrl}");
            var ws = new ClientWebSocket();
            //ws.Options.RemoteCertificateValidationCallback = delegate { return true; };
            await ws.ConnectAsync(wsUrl, CancellationToken.None);
            var buffer = new byte[1024 * 4];
            string input = "";
            while (true)
            {
                Console.Write(">> ");
                input = Console.ReadLine() ?? "";
                await ws.SendAsync(Encoding.UTF8.GetBytes(input), WebSocketMessageType.Text, true, CancellationToken.None);
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var text = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"<< {text}");
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }
}
