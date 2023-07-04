using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cli.Utils;
using Timer = System.Timers.Timer;

namespace Cli.Commands.ken_wp;

public class WebPingCommand
{
    /// <summary>
    /// 需要测试的地址
    /// </summary>
    private static readonly Argument<string> Url = new("url", "url: https://www.kentxxq.com");

    private static readonly Option<double> Interval = new(new[] { "-i", "--interval" }, () => 1,
        "web ping interval seconds");

    private static readonly Option<int> Timeout = new(new[] { "-t", "--timeout" }, () => 2, "default:2 seconds");

    private static readonly Option<bool> DisableKeepAlive =
        new(new[] { "-d", "--disableKeepAlive" }, () => false, "default: true");

    // 因为会包含多行,单引号,双引号.所以放到文件里才能读取
    private static readonly Option<FileInfo?> CurlFile = new(new[] { "-f","--curlFile" }, () => null, "if curlFile is not null ,Argument url will be ignore. default: ''");

    private static readonly HttpClient _client = new();

    // private static readonly Stopwatch _stopwatch = new();


    public static Command GetCommand()
    {
        var command = new Command("wp", "web ping")
        {
            Url,
            Interval,
            Timeout,
            DisableKeepAlive,
            CurlFile
        };
        command.SetHandler(async context =>
        {
            var url = context.ParseResult.GetValueForArgument(Url);
            var interval = context.ParseResult.GetValueForOption(Interval);
            var timeout = context.ParseResult.GetValueForOption(Timeout);
            var disableKeepAlive = context.ParseResult.GetValueForOption(DisableKeepAlive);
            var curlFile = context.ParseResult.GetValueForOption(CurlFile);
            
            if (disableKeepAlive)
            {
                _client.DefaultRequestHeaders.ConnectionClose = true;
            }
            await Run(url, interval, timeout,curlFile);
            Console.ReadKey();
        });
        return command;
    }

    private static async Task Run(string url, double interval, int timeout,FileInfo? curlFile)
    {
        // Console.WriteLine(String.Concat(Enumerable.Repeat("=", 50)));
        // var rule = new Rule("ken-wp");
        // rule.Alignment = Justify.Left;
        // rule.RuleStyle("red dim");
        // AnsiConsole.Write(rule);

        HttpRequestMessage? request;
        var curlCommand = string.Empty;
        if (curlFile is not null && curlFile.Exists)
        {
            curlCommand = await File.ReadAllTextAsync(curlFile.FullName);
        }
        
        var timer = new Timer(interval * 1000);
        timer.AutoReset = true;
        timer.Elapsed += async (sender, e) =>
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
            await SendRequest(request, timeout);
        };
        timer.Enabled = true;
    }

    private static async Task SendRequest(HttpRequestMessage httpRequestMessage, int timeout)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(timeout));
        var stopWatch = new Stopwatch();
        try
        {
            stopWatch.Start();
            var httpResponseMessage = await _client.SendAsync(httpRequestMessage,cts.Token);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                MyLog.Logger?.Debug("请求成功");
                MyLog.Logger?.Debug(await HttpTools.HttpResponseMessageToString(httpResponseMessage));
            }
            
            MyAnsiConsole.MarkupSuccessLine(
                $"{DateTime.Now:hh:mm:ss},{httpRequestMessage.RequestUri}: {httpResponseMessage.StatusCode} {stopWatch.ElapsedMilliseconds}ms");
        }
        catch (Exception e)
        {
            MyAnsiConsole.MarkupErrorLine(
                $"{DateTime.Now:hh:mm:ss},err: {e.Message} {stopWatch.ElapsedMilliseconds}ms");
        }
        finally
        {
            stopWatch.Stop();
            stopWatch.Reset();
        }
    }
}