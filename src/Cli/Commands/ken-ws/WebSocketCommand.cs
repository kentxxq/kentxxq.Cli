using System.CommandLine;
using System.Net.WebSockets;
using System.Text;
using Cli.Utils;

namespace Cli.Commands.ken_ws;

internal static class WebSocketCommand
{
    private static readonly Argument<string> WebSocketUrl = new("wsUrl") { Description = "ws:// 或 wss:// 地址" };

    public static Command GetCommand()
    {
        var command = new Command("ws", "WebSocket 交互连接") { WebSocketUrl };
        command.SetAction(async (result, ct) =>
        {
            if (!Uri.TryCreate(result.GetValue(WebSocketUrl), UriKind.Absolute, out var url) || url.Scheme is not ("ws" or "wss"))
                throw new ArgumentException("需要有效的 WebSocket 地址。");
            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(url, ct);
            var buffer = new byte[4096];
            while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                Console.Write(">> ");
                var input = await Console.In.ReadLineAsync(ct);
                if (input is null) break;
                await ws.SendAsync(Encoding.UTF8.GetBytes(input), WebSocketMessageType.Text, true, ct);
                using var message = new MemoryStream();
                WebSocketReceiveResult received;
                do
                {
                    received = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (received.MessageType == WebSocketMessageType.Close)
                    {
                        using var closeDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                        await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", closeDeadline.Token);
                        return;
                    }
                    message.Write(buffer, 0, received.Count);
                } while (!received.EndOfMessage);
                MyAnsiConsole.MarkupSuccessLine(received.MessageType == WebSocketMessageType.Text
                    ? "<< " + Encoding.UTF8.GetString(message.ToArray())
                    : $"<< 二进制消息，{message.Length} 字节");
            }
            if (ws.State == WebSocketState.Open)
            {
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", deadline.Token);
            }
            ct.ThrowIfCancellationRequested();
        });
        return command;
    }
}
