using Cli.Commands.ken_sp;
using Cli.Commands.ken_ss;
using Cli.Commands.ken_tr;
using Cli.Commands.ken_ws;
using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return new CommandLineBuilder(rootCommand);
        }
    }
}
