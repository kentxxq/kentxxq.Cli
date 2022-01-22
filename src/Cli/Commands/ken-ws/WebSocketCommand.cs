using Cli.Utils;
using System;
using System.CommandLine;
using System.Diagnostics;
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
            var command = new Command("ws", "websocket connect");
            command.AddArgument(wsUrl);

            command.SetHandler<Uri, CancellationToken>(Run, wsUrl);
            return command;
        }

        private static async Task Run(Uri wsUrl, CancellationToken ct)
        {
            var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(wsUrl, ct);
            }
            catch (Exception e)
            {
                MyAnsiConsole.MarkupErrorLine($"connect faild:{e.Message}");
                return;
            }

            var buffer = new byte[1024 * 4];
            string input;
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Process.GetCurrentProcess().Kill();
            };
            while (!ct.IsCancellationRequested)
            {
                Console.Write(">> ");
                input = Console.ReadLine() ?? "";
                await ws.SendAsync(Encoding.UTF8.GetBytes(input), WebSocketMessageType.Text, true, ct);

                await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                var text = Encoding.UTF8.GetString(buffer);
                MyAnsiConsole.MarkupSuccessLine($"<< {text}");
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }
}
