using System.CommandLine;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Cli.Utils;

namespace Cli.Commands.ken_sp;

public static class SocketPingCommand
{
    private static readonly Argument<string> Url = new("url") { Description = "主机:端口，IPv6 使用 [::1]:443" };
    private static readonly Option<int> RetryTimes = new("--retryTimes", "-n") { DefaultValueFactory = _ => 0, Description = "次数，0 表示持续运行" };
    private static readonly Option<int> Timeout = new("--timeout", "-t") { DefaultValueFactory = _ => 2, Description = "连接超时秒数" };
    private static readonly Option<bool> Quit = new("--quit", "-q") { Description = "连接成功后退出" };

    public static Command GetCommand()
    {
        var command = new Command("sp", "TCP 连通性检查") { Url, RetryTimes, Timeout, Quit };
        command.SetAction(async (result, ct) =>
        {
            var count = result.GetValue(RetryTimes);
            var timeout = result.GetValue(Timeout);
            if (count < 0 || timeout <= 0) throw new ArgumentException("次数不能为负数，超时必须大于零。");
            if (!Uri.TryCreate("tcp://" + result.GetValue(Url), UriKind.Absolute, out var endpoint)
                || endpoint.Port is < 1 or > 65535 || endpoint.AbsolutePath != "/" || endpoint.UserInfo.Length != 0)
                throw new ArgumentException("请提供主机:端口。");
            using var dnsTimeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            dnsTimeout.CancelAfter(TimeSpan.FromSeconds(timeout));
            var addresses = await Dns.GetHostAddressesAsync(endpoint.DnsSafeHost, dnsTimeout.Token);
            var succeeded = false;
            for (var i = 0; count == 0 || i < count; i++)
            {
                ct.ThrowIfCancellationRequested();
                succeeded = await Connect(addresses, endpoint.Port, timeout, ct);
                if (succeeded && result.GetValue(Quit)) return 0;
                if (count == 0 || i + 1 < count) await Task.Delay(500, ct);
            }
            return succeeded ? 0 : 1;
        });
        return command;
    }

    private static async Task<bool> Connect(IPAddress[] addresses, int port, int timeout, CancellationToken ct)
    {
        using var tcp = new TcpClient();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(timeout));
        var watch = Stopwatch.StartNew();
        try
        {
            await tcp.ConnectAsync(addresses, port, deadline.Token);
            MyAnsiConsole.MarkupSuccessLine($"连接成功，耗时 {watch.ElapsedMilliseconds}ms");
            return true;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            MyAnsiConsole.MarkupErrorLine($"连接超时，耗时 {watch.ElapsedMilliseconds}ms");
            return false;
        }
        catch (SocketException)
        {
            MyAnsiConsole.MarkupErrorLine($"连接失败，耗时 {watch.ElapsedMilliseconds}ms");
            return false;
        }
    }
}
