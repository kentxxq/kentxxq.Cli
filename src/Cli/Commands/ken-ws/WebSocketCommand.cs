using System;
using System.CommandLine;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cli.Utils;

namespace Cli.Commands.ken_ws;

internal static class WebSocketCommand
{
    private static readonly Argument<Uri> WebSocketUrl = new("wsUrl", "wsUrl: wss://ws.kentxxq.com/ws");

    public static Command GetCommand()
    {
        var command = new Command("ws", "websocket connect");
        command.AddArgument(WebSocketUrl);

        command.SetHandler(async context =>
        {
            var wsUrl = context.ParseResult.GetValueForArgument(WebSocketUrl);
            var ct = context.GetCancellationToken();
            await Run(wsUrl, ct);
        });

        return command;
    }

    private static async Task Run(Uri wsUrl, CancellationToken ct)
    {
        var ws = new ClientWebSocket();
        await ws.ConnectAsync(wsUrl, ct);

        var buffer = new byte[1024 * 4];
        Console.CancelKeyPress += (_, _) => { Process.GetCurrentProcess().Kill(); };
        while (!ct.IsCancellationRequested)
        {
            Console.Write(">> ");
            var input = Console.ReadLine() ?? "";
            await ws.SendAsync(Encoding.UTF8.GetBytes(input), WebSocketMessageType.Text, true, ct);

            await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
            var text = Encoding.UTF8.GetString(buffer);
            MyAnsiConsole.MarkupSuccessLine($"<< {text}");
            Array.Clear(buffer, 0, buffer.Length);
        }
    }
}