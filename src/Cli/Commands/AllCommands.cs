using System.CommandLine;

using Cli.Commands.ken_bm;
using Cli.Commands.ken_k8s;
using Cli.Commands.ken_mirror;
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
    private static readonly Option<bool> Debug = new("--debug") { Description = "enable verbose output", DefaultValueFactory = _ => false };

    public static RootCommand BuildCommandLine()
    {
        var rootCommand = new RootCommand();
        Debug.Recursive = true;
        rootCommand.Options.Add(Debug);
        rootCommand.Subcommands.Add(SocketPingCommand.GetCommand());
        rootCommand.Subcommands.Add(WebSocketCommand.GetCommand());
        rootCommand.Subcommands.Add(SocketStatisticsCommand.GetCommand());
        rootCommand.Subcommands.Add(TracerouteCommand.GetCommand());
        rootCommand.Subcommands.Add(RedisCommand.GetCommand());
        rootCommand.Subcommands.Add(K8SCommand.GetCommand());
        rootCommand.Subcommands.Add(WebCommand.GetCommand());
        rootCommand.Subcommands.Add(UpdateCommand.GetCommand());
        rootCommand.Subcommands.Add(WebPingCommand.GetCommand());
        rootCommand.Subcommands.Add(BenchMarkCommand.GetCommand());
        rootCommand.Subcommands.Add(MirrorCommand.GetCommand());
        return rootCommand;
    }
}