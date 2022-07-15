using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Masuit.Tools;

namespace Cli.Commands.ken_bm;

public class BenchMarkCommand
{
    /// <summary>
    /// http地址
    /// </summary>
    private static readonly Argument<string> Url = new("url", "url: https://test.kentxxq.com/api/Counter/Count");
    
    private static int count = 0;

    public static Command GetCommand()
    {
        var command = new Command("bm","http benchmark")
        {
            Url
        };
        command.SetHandler(async context =>
        {
            var url = context.ParseResult.GetValueForArgument(Url);
            // var cts = context.GetCancellationToken();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            await Run(url,cts.Token);
        });
        return command;
    }

    private static async Task Run(string url,CancellationToken cts)
    {
        var client = new HttpClient()
        {
            BaseAddress = new Uri(url)
        };
        
        var taskList = new List<Task<string>>();
        for (int i = 0; i < 50; i++)
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