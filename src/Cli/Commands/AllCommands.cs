using Cli.Commands.ken_redis;
using Cli.Commands.ken_sp;
using Cli.Commands.ken_ss;
using Cli.Commands.ken_tr;
using Cli.Commands.ken_ws;
using System.CommandLine;
using System.CommandLine.Builder;

namespace Cli.Commands
{
    public class AllCommands
    {
        public static CommandLineBuilder BuildCommandLine()
        {
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(SocketPingCommand.GetCommand());
            rootCommand.AddCommand(WebSocketCommand.GetCommand());
            rootCommand.AddCommand(SocketStatisticsCommand.GetCommand());
            rootCommand.AddCommand(TracerouteCommand.GetCommand());
            rootCommand.AddCommand(RedisCommand.GetCommand());
            return new CommandLineBuilder(rootCommand);
        }
    }
}
