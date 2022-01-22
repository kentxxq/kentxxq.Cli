using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;


var result = await Cli.Commands.AllCommands.BuildCommandLine()
      .UseDefaults()
      .Build()
#if DEBUG
//.InvokeAsync(new string[] { "ss" });
//.InvokeAsync(new string[] { "tr", "kentxxq.com" });
//.InvokeAsync(new string[] { "ws", "wss://ws.kentxxq.com/ws" });
.InvokeAsync(new string[] { "sp", "kentxxq.com:443", "-t 2", "-q", });
#else
      .InvokeAsync(args);
      //.InvokeAsync(new string[] { "tr" });
#endif
//System.Console.WriteLine(result);
