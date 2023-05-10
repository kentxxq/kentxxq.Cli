using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Cli.Commands;
using Cli.Utils;

await AllCommands.BuildCommandLine()
    .UseDefaults()
    .UseExceptionHandler((exception, context) => { MyAnsiConsole.MarkupErrorLine($"{exception.Message}"); }, 1)
    .Build()
#if DEBUG
    // .InvokeAsync(new string[] { "ss" });
    // .InvokeAsync(new[] { "tr", "baidu.com" });
// .InvokeAsync(new[] { "ws", "wss://ws.kentxxq.com/ws" });
.InvokeAsync(new[] { "sp", "kentxxq.com:443", "-t 2","-n 5" });
// .InvokeAsync(new[] { "redis", "a.kentxxq.com","-p didi2" });
// .InvokeAsync(new[] { "k8s", "2" });
// .InvokeAsync(new[] { "wp", "https://test.kentxxq.com/api/Delay/1500","-t 2","-i 5" });
// .InvokeAsync(new[] { "bm", "https://test.kentxxq.com/api/Counter/count" });
// .InvokeAsync(new[] { "bm", "http://127.0.0.1:5000/" });
// .InvokeAsync(new[] { "web" });
// .InvokeAsync(new[] { "update", "-f", "-p Ghproxy", "--debug" });
#else
      .InvokeAsync(args);
// .InvokeAsync(new[] { "update", "-f" });
#endif
//System.Console.WriteLine(result);