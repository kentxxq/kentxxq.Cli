using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cli.Utils;
using Masuit.Tools;

namespace Cli.Commands.ken_bm;

/// <summary>
/// TODO 研究和hey之类的工具差距
/// </summary>
public class BenchMarkCommand
{
    /// <summary>
    /// http地址
    /// </summary>
    private static readonly Argument<string> Url = new("url", "url: https://uni.kentxxq.com/Counter/Count");

    private static readonly Option<int> Duration = new(new[] { "-d", "--duration" }, () => 10,
        "duration: benchmark duration");

    private static readonly Option<int> Concurrent = new(new[] { "-c", "--concurrent" }, () => 50,
        "concurrent: concurrent request");

    // 因为会包含多行,单引号,双引号.所以放到文件里才能读取
    private static readonly Option<FileInfo?> CurlFile = new(new[] { "-f","--curlFile" }, () => null, "if curlFile is not null ,Argument url will be ignore. default: ''");

    private static int count;

    public static Command GetCommand()
    {
        var command = new Command("bm", "http benchmark")
        {
            Url,
            Duration,
            Concurrent,
            CurlFile
        };
        command.SetHandler(async context =>
        {
            var url = context.ParseResult.GetValueForArgument(Url);
            var duration = context.ParseResult.GetValueForOption(Duration);
            var concurrent = context.ParseResult.GetValueForOption(Concurrent);
            var curlFile = context.ParseResult.GetValueForOption(CurlFile);
            // var cts = context.GetCancellationToken();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(duration));
            await Run(url, concurrent,curlFile, cts.Token);
            // await Run2(url, cts.Token);
        });
        return command;
    }

    // private static async Task Run2(string url, CancellationToken cts)
    // {
    //     var client = new HttpClient()
    //     {
    //         BaseAddress = new Uri(url)
    //     };
    //     var stopWatch = new Stopwatch();
    //     stopWatch.Start();
    //     var taskList = new List<Task<string>>();
    //     for (int i = 0; i < 50000; i++)
    //     {
    //         var t = client.GetStringAsync(url,cts);
    //         taskList.Add(t);
    //     }
    //
    //     await Task.WhenAll(taskList);
    //     Console.WriteLine($"{stopWatch.ElapsedMilliseconds / 1000} s");
    // }

    private static async Task Run(string url, int concurrent,FileInfo? curlFile, CancellationToken cts)
    {
        HttpRequestMessage? request;
        var curlCommand = string.Empty;
        if (curlFile is not null && curlFile.Exists)
        {
            curlCommand = await File.ReadAllTextAsync(curlFile.FullName, cts);
        }

        var client = new HttpClient
        {
            BaseAddress = new Uri(url)
        };

        var taskList = new List<Task<HttpResponseMessage>>();
        for (var i = 0; i < concurrent; i++)
        {
            if (!string.IsNullOrEmpty(curlCommand))
            {
                request = await HttpTools.CurlToHttpRequestMessage(curlCommand);
                if (request is null)
                {
                    MyAnsiConsole.MarkupErrorLine("curl parse error!");
                    return;
                }
            }
            else
            {
                request = new HttpRequestMessage(HttpMethod.Get, url);
            }
            var t = client.SendAsync(request, cts);
            taskList.Add(t);
        }

        while (!cts.IsCancellationRequested)
        {
            await Task.WhenAny(taskList);
            var finishTask = taskList.Where(t => t.IsCompleted).ToList();
            count += finishTask.Count; // TODO 使用结果集
            taskList.RemoveWhere(t => finishTask.Contains(t));

            for (var i = 0; i < finishTask.Count; i++)
            {
                if (!string.IsNullOrEmpty(curlCommand))
                {
                    request = await HttpTools.CurlToHttpRequestMessage(curlCommand);
                    if (request is null)
                    {
                        MyAnsiConsole.MarkupErrorLine("curl parse error!");
                        return;
                    }
                }
                else
                {
                    request = new HttpRequestMessage(HttpMethod.Get, url);
                }
                var t = client.SendAsync(request, cts);
                taskList.Add(t);
            }
        }

        Console.WriteLine($"完成次数{count}");
    }
}
