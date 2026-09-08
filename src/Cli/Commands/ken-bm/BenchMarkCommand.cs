using System.CommandLine;
using System.Diagnostics;
using Cli.Utils;

namespace Cli.Commands.ken_bm;

public static class BenchMarkCommand
{
    private static readonly Argument<string?> Url = new("url") { Arity = ArgumentArity.ZeroOrOne, Description = "HTTP 地址；使用 -f 时可省略" };
    private static readonly Option<int> Duration = new("--duration", "-d") { DefaultValueFactory = _ => 10, Description = "持续秒数" };
    private static readonly Option<int> Concurrent = new("--concurrent", "-c") { DefaultValueFactory = _ => 50, Description = "并发请求数" };
    private static readonly Option<FileInfo?> CurlFile = new("--curlFile", "-f") { Description = "从 curl 文件读取请求" };

    public static Command GetCommand()
    {
        var command = new Command("bm", "HTTP 压测") { Url, Duration, Concurrent, CurlFile };
        command.SetAction(async (result, ct) =>
        {
            var duration = result.GetValue(Duration);
            var concurrent = result.GetValue(Concurrent);
            if (duration <= 0 || concurrent <= 0) throw new ArgumentException("持续时间和并发数必须大于零。");
            var curl = await HttpTools.ReadCurlFile(result.GetValue(CurlFile), ct);
            using var validation = await HttpTools.CreateRequest(result.GetValue(Url), curl);
            using var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
            deadline.CancelAfter(TimeSpan.FromSeconds(duration));
            long success = 0, httpFailure = 0, connectionFailure = 0, unfinished = 0;
            var watch = Stopwatch.StartNew();
            async Task Worker()
            {
                while (!deadline.IsCancellationRequested)
                {
                    using var request = await HttpTools.CreateRequest(result.GetValue(Url), curl);
                    try
                    {
                        using var response = await client.SendAsync(request, deadline.Token);
                        if (response.IsSuccessStatusCode) Interlocked.Increment(ref success);
                        else Interlocked.Increment(ref httpFailure);
                    }
                    catch (OperationCanceledException) when (deadline.IsCancellationRequested) { Interlocked.Increment(ref unfinished); break; }
                    catch (HttpRequestException) { Interlocked.Increment(ref connectionFailure); }
                }
            }
            await Task.WhenAll(Enumerable.Range(0, concurrent).Select(_ => Worker()));
            MyAnsiConsole.MarkupSuccessLine($"成功 {success}，HTTP 失败 {httpFailure}，连接失败 {connectionFailure}，截止时未完成 {unfinished}，耗时 {watch.Elapsed.TotalSeconds:F2}s");
            ct.ThrowIfCancellationRequested();
            return success > 0 && httpFailure + connectionFailure == 0 ? 0 : 1;
        });
        return command;
    }
}
