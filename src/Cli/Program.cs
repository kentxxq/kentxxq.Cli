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
#if DEBUG
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
                  .InvokeAsync(new string[] { "sp", "baidu.com:443", "-t 2", "-n 10", });
            System.Console.WriteLine(result);
#else
            await BuildCommandLine()
                  .UseHost(_ => Host.CreateDefaultBuilder(), builder =>
                  {
                      builder.ConfigureServices(service =>
                      {

                      });
                  })
                  .UseDefaults()
                  .Build()
                  .InvokeAsync(args);
#endif
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
