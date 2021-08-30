using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Cli.Commands.ken_sp;
using Cli.Commands.ken_ws;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                      builder.ConfigureServices(service =>
                      {

                      });
                  })
                  .UseDefaults()
                  .Build()
#if DEBUG
            //.InvokeAsync(new string[] { "ws", "wss://ws.kentxxq.com/ws" });
            .InvokeAsync(new string[] { "sp", "kentxxq.com:443", "-t 2", "-n 10", });
#else
                  .InvokeAsync(args);
#endif
            //System.Console.WriteLine(result);

        }

        private static CommandLineBuilder BuildCommandLine()
        {
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(SocketPingCommand.GetCommand());
            rootCommand.AddCommand(WebSocketCommand.GetCommand());
            return new CommandLineBuilder(rootCommand);
        }
    }
}
