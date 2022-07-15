using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Masuit.Tools;

namespace Cli.Commands.ken_bm;

/// <summary>
/// TODO 性能和hey还是有差距。。。
/// </summary>
public class BenchMarkCommand
{
    /// <summary>
    /// http地址
    /// </summary>
    private static readonly Argument<string> Url = new("url", "url: https://test.kentxxq.com/api/Counter/Count");

    private static readonly Option<int> Duration = new(new string[] { "-d", "--duration"}, () => 10, "duration: benchmark duration");
    
    private static readonly Option<int> Concurrent = new(new string[] { "-c", "--concurrent"}, () => 50, "concurrent: concurrent request");

    private static int count = 0;

    public static Command GetCommand()
    {
        var command = new Command("bm","http benchmark")
        {
            Url,
            Duration,
            Concurrent
        };
        command.SetHandler(async context =>
        {
            var url = context.ParseResult.GetValueForArgument(Url);
            var duration = context.ParseResult.GetValueForOption(Duration);
            var concurrent = context.ParseResult.GetValueForOption(Concurrent);
            // var cts = context.GetCancellationToken();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(duration));
            await Run(url,concurrent,cts.Token);
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
    
    private static async Task Run(string url,int concurrent,CancellationToken cts)
    {
        var client = new HttpClient()
        {
            BaseAddress = new Uri(url)
        };
        
        var taskList = new List<Task<string>>();
        for (int i = 0; i < concurrent; i++)
        {
            var t = client.GetStringAsync(url,cts);
            taskList.Add(t);
        }

        while (!cts.IsCancellationRequested)
        {
            await Task.WhenAny(taskList);
            var finishTask = taskList.Where(t => t.IsCompleted).ToList();
            count += finishTask.Count;// TODO 使用结果集
            taskList.RemoveWhere(t=>finishTask.Contains(t));
            
            for (int i = 0; i < finishTask.Count; i++)
            {
                var t = client.GetStringAsync(url,cts);
                taskList.Add(t);
            }
        }
        Console.WriteLine($"完成次数{count}");
    }
}