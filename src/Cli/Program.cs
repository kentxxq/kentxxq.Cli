using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Cli.Commands;
using Cli.Utils;

await AllCommands.BuildCommandLine()
    .UseDefaults()
    .UseExceptionHandler((exception, context) =>
    {
        MyAnsiConsole.MarkupErrorLine($"{exception.Message}");
    },1)
    .Build()
#if DEBUG
    // .InvokeAsync(new string[] { "ss" });
    // .InvokeAsync(new[] { "tr", "kentxxq.com" });
// .InvokeAsync(new[] { "ws", "wss://ws.kentxxq.com/ws" });
// .InvokeAsync(new[] { "sp", "kentxxq.com:443", "-t 2", "-q" });
// .InvokeAsync(new[] { "redis", "bwd.kentxxq.com","-p didi" });
    // .InvokeAsync(new[] { "k8s", "get-restarted-pod" });
    // .InvokeAsync(new[] { "web" });
    .InvokeAsync(new[] { "update", "-f" });
#else
      .InvokeAsync(args);
      //.InvokeAsync(new string[] { "tr" });
#endif
//System.Console.WriteLine(result);