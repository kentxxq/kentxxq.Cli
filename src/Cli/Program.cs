using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using Cli.Interfaces;
using Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;


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
