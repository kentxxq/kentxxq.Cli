using System;
using System.CommandLine;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Cli.Utils;

namespace Cli.Commands.ken_wp;

public class WebPingCommand
{
    /// <summary>
    /// 需要测试的地址
    /// </summary>
    private static readonly Argument<string> Url = new("url", "url: https://www.kentxxq.com");

    private static readonly Option<double> Interval = new(new[] { "-i", "--interval" },()=>1, "web ping interval seconds"); 
    
    private static readonly Option<int> Timeout = new(new[] { "-t", "--timeout" }, () => 2, "default:2 seconds"); 
    
    private static readonly HttpClient _client = new();

    // private static readonly Stopwatch _stopwatch = new();


    public static Command GetCommand()
    {
        var command = new Command("wp", "web ping")
        {
            Url,
            Interval,
            Timeout
        };
        command.SetHandler(context =>
        {
            var url = context.ParseResult.GetValueForArgument(Url);
            var interval = context.ParseResult.GetValueForOption(Interval);
            var timeout = context.ParseResult.GetValueForOption(Timeout);
            Run(url,interval,timeout);
            Console.ReadKey();
        });
        return command;
    }

    private static void Run(string url,double interval,int timeout)
    {
        // Console.WriteLine(String.Concat(Enumerable.Repeat("=", 50)));
        // var rule = new Rule("ken-wp");
        // rule.Alignment = Justify.Left;
        // rule.RuleStyle("red dim");
        // AnsiConsole.Write(rule);

        var timer = new System.Timers.Timer(interval*1000);
        timer.AutoReset = true;
        timer.Elapsed += async ( sender, e ) => await SendRequest(url,timeout);
        timer.Enabled = true;
    }

    private static async Task SendRequest(string url,int timeout)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(timeout));
        var stopWatch = new Stopwatch();
        try
        {
            stopWatch.Start();
            var httpResponseMessage = await _client.GetAsync(url, cts.Token);
            // var data = await _client.GetStringAsync(url, cts.Token);
            // if (!data.Contains("9999"))
            // {
            //     Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss")},ok,{stopWatch.ElapsedMilliseconds}ms");
            // }
            // else
            // {
            //     Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss")},error,{stopWatch.ElapsedMilliseconds}ms");
            // }
            MyAnsiConsole.MarkupSuccessLine($"{DateTime.Now.ToString("hh:mm:ss")},{url}: {httpResponseMessage.StatusCode} {stopWatch.ElapsedMilliseconds}ms");
        }
        catch (Exception e)
        {
            MyAnsiConsole.MarkupErrorLine($"{DateTime.Now.ToString("hh:mm:ss")},err: {e.Message} {stopWatch.ElapsedMilliseconds}ms");
        }
        finally
        {
            stopWatch.Stop();
            stopWatch.Reset();
        }
    }
}