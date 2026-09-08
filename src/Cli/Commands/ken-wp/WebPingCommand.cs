using System.CommandLine;
using System.Diagnostics;
using Cli.Utils;

namespace Cli.Commands.ken_wp;

public static class WebPingCommand
{
    private static readonly Argument<string?> Url = new("url") { Arity = ArgumentArity.ZeroOrOne, Description = "HTTP 地址；使用 -f 时可省略" };
    private static readonly Option<double> Interval = new("--interval", "-i") { DefaultValueFactory = _ => 1, Description = "请求间隔秒数" };
    private static readonly Option<int> Timeout = new("--timeout", "-t") { DefaultValueFactory = _ => 2, Description = "请求超时秒数" };
    private static readonly Option<bool> DisableKeepAlive = new("--disableKeepAlive", "-d") { Description = "关闭连接复用，默认 false" };
    private static readonly Option<FileInfo?> CurlFile = new("--curlFile", "-f") { Description = "从 curl 文件读取请求" };

    public static Command GetCommand()
    {
        var command = new Command("wp", "持续检查 HTTP 响应") { Url, Interval, Timeout, DisableKeepAlive, CurlFile };
        command.SetAction(async (result, ct) =>
        {
            var interval = result.GetValue(Interval);
            var timeout = result.GetValue(Timeout);
            if (!double.IsFinite(interval) || interval <= 0 || interval > int.MaxValue / 1000 || timeout <= 0)
                throw new ArgumentException("间隔和超时必须为有效正数。");
            var curl = await HttpTools.ReadCurlFile(result.GetValue(CurlFile), ct);
            using var client = new HttpClient { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
            using var validation = await HttpTools.CreateRequest(result.GetValue(Url), curl);
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                using var request = await HttpTools.CreateRequest(result.GetValue(Url), curl);
                request.Headers.ConnectionClose = result.GetValue(DisableKeepAlive);
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
                deadline.CancelAfter(TimeSpan.FromSeconds(timeout));
                var watch = Stopwatch.StartNew();
                try
                {
                    using var response = await client.SendAsync(request, deadline.Token);
                    var message = $"{DateTime.Now:HH:mm:ss.fff} {(int)response.StatusCode}，耗时 {watch.ElapsedMilliseconds}ms";
                    if (response.IsSuccessStatusCode) MyAnsiConsole.MarkupSuccessLine(message);
                    else MyAnsiConsole.MarkupWarningLine(message);
                    MyLog.Logger?.Debug("HTTP 状态码：{StatusCode}", (int)response.StatusCode);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    MyAnsiConsole.MarkupErrorLine($"请求超时，耗时 {watch.ElapsedMilliseconds}ms");
                }
                catch (HttpRequestException)
                {
                    MyAnsiConsole.MarkupErrorLine($"请求失败，耗时 {watch.ElapsedMilliseconds}ms");
                }
                // 请求不重叠；间隔从本次请求结束开始计算。
                await Task.Delay(TimeSpan.FromSeconds(interval), ct);
            }
        });
        return command;
    }
}
