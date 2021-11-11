using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Threading.Tasks;
using Cli.Commands.ken_sp;
using Cli.Commands.ken_ss;
using Cli.Commands.ken_tr;
using Cli.Commands.ken_ws;
using Cli.Interfaces;
using Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Cli
{
    class Program
    {

        static async Task Main(string[] args)
        {

            var result = await BuildCommandLine()
                  .UseHost(_ => Host.CreateDefaultBuilder(), builder =>
                  {
                      builder.ConfigureLogging(logging => logging
                                                            .AddFilter("System", LogLevel.Error)
                                                            .AddFilter("Microsoft", LogLevel.Error));
                      builder.ConfigureServices(services =>
                      {
                          var console = new SystemConsole();
                          var consoleRender = new ConsoleRenderer(console, OutputMode.Ansi, true);
                          services.AddSingleton(consoleRender);

                          services.AddHttpClient<IIpService, IpService>()
                                  .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(1)));
                          services.AddTransient<IConnectService, ConnectService>();
                      });
                  })
                  .UseDefaults()
                  .Build()
#if DEBUG
            //.InvokeAsync(new string[] { "ss" });
            .InvokeAsync(new string[] { "tr", "kentxxq.com" });
            //.InvokeAsync(new string[] { "ws", "wss://ws.kentxxq.com/ws" });
            //.InvokeAsync(new string[] { "sp", "kentxxq.com:443", "-t 2", "-n 10", });
#else
                  .InvokeAsync(args);
                  //.InvokeAsync(new string[] { "tr" });
#endif
            //System.Console.WriteLine(result);

        }

        private static CommandLineBuilder BuildCommandLine()
        {
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(SocketPingCommand.GetCommand());
            rootCommand.AddCommand(WebSocketCommand.GetCommand());
            rootCommand.AddCommand(SocketStatisticsCommand.GetCommand());
            rootCommand.AddCommand(TracerouteCommand.GetCommand());
            return new CommandLineBuilder(rootCommand);
        }
    }
}
