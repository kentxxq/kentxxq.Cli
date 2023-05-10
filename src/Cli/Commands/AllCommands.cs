using System.CommandLine;
using System.CommandLine.Builder;
using Cli.Commands.ken_bm;
using Cli.Commands.ken_k8s;
using Cli.Commands.ken_redis;
using Cli.Commands.ken_sp;
using Cli.Commands.ken_ss;
using Cli.Commands.ken_tr;
using Cli.Commands.ken_update;
using Cli.Commands.ken_web;
using Cli.Commands.ken_wp;
using Cli.Commands.ken_ws;

namespace Cli.Commands;

public class AllCommands
{
    /// <summary>
    /// 连接成功后是否退出
    /// </summary>
    private static readonly Option<bool> Debug = new(new[] { "--debug" }, () => false,
        "enable verbose output");
    
    public static CommandLineBuilder BuildCommandLine()
    {
        var rootCommand = new RootCommand();
        rootCommand.AddGlobalOption(Debug);
        rootCommand.AddCommand(SocketPingCommand.GetCommand());
        rootCommand.AddCommand(WebSocketCommand.GetCommand());
        rootCommand.AddCommand(SocketStatisticsCommand.GetCommand());
        rootCommand.AddCommand(TracerouteCommand.GetCommand());
        rootCommand.AddCommand(RedisCommand.GetCommand());
        rootCommand.AddCommand(K8SCommand.GetCommand());
        rootCommand.AddCommand(WebCommand.GetCommand());
        rootCommand.AddCommand(UpdateCommand.GetCommand());
        rootCommand.AddCommand(WebPingCommand.GetCommand());
        rootCommand.AddCommand(BenchMarkCommand.GetCommand());
        return new CommandLineBuilder(rootCommand);
    }
}