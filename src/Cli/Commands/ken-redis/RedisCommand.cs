using System;
using System.CommandLine;
using System.Linq;
using System.Threading;
using Cli.Utils;
using Spectre.Console;
using StackExchange.Redis;

namespace Cli.Commands.ken_redis;

public static class RedisCommand
{
    /// <summary>
    /// redis连接地址
    /// </summary>
    private static readonly Argument<string> Url = new("url", "url: redis.com");

    /// <summary>
    /// redis的端口号，默认6379
    /// </summary>
    private static readonly Option<int> Port = new(new[] { "-port", "--serverPort" }, () => 6379, "default:6379");

    /// <summary>
    /// 连接的db，默认为0
    /// </summary>
    private static readonly Option<int> Db = new(new[] { "-db", "--database" }, () => 0, "default:0");

    /// <summary>
    /// 连接redis时的密码，默认为空
    /// </summary>
    private static readonly Option<string> Password = new(new[] { "-p", "--password" }, () => "", "default empty");

    public static Command GetCommand()
    {
        var command = new Command("redis", "redis")
        {
            Url,
            Port,
            Db,
            Password
        };

        command.SetHandler(context =>
        {
            var url = context.ParseResult.GetValueForArgument(Url);
            var port = context.ParseResult.GetValueForOption(Port);
            var db = context.ParseResult.GetValueForOption(Db);
            var password = context.ParseResult.GetValueForOption(Password)!.Replace(" ", "");
            var ct = context.GetCancellationToken();
            Run(url, port, db, password, ct);
        });

        return command;
    }


    private static void Run(string url, int port, int db, string password, CancellationToken ct)
    {
        var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { $"{url}:{port}" },
            ConnectRetry = 2,
            ConnectTimeout = 3000,
            Password = password
        });

        // 测试连接
        var dbc = redis.GetDatabase(db);
        var pong = dbc.Ping();
        AnsiConsole.MarkupLine($"connect [green]success[/],take {pong.TotalMilliseconds} ms");

        // 打印此db的keys数量
        var server = redis.GetServer(url, port);
        var keysCount = server.Keys(db, "*").Count();
        // Console.WriteLine($"db{db} keys:{keysCount}");
        AnsiConsole.MarkupLine($"[green]db{db}[/] keys count:[green]{keysCount}[/]");

        int n;
        while (!ct.IsCancellationRequested)
        {
            Console.Write(">");
            var input = Console.ReadLine() ?? "";
            var inputs = input.Split(" ", 2);

            // 如果输入的是一个非空字符串，且不是exit()，那么就是查询操作
            if (inputs.Length == 1 && input != "" && input != "exit()")
            {
                var keys = server.Keys(db, input);
                MyAnsiConsole.MarkupSuccessLine($"keys count:{keys.Count()}");
            }
            else
            {
                switch (inputs[0])
                {
                    case "del":
                        var delKeys = server.Keys(db, inputs[1]);
                        var delKeysCount = dbc.KeyDelete(delKeys.ToArray());
                        MyAnsiConsole.MarkupErrorLine($"deleted {delKeysCount} key(s)");
                        break;
                    case "select":
                        if (int.TryParse(inputs[1], out n))
                        {
                            db = n;
                            AnsiConsole.MarkupLine(
                                $"using [green]db{inputs[1]} [/]keys count:[green]{server.Keys(db, "*").Count()}[/]");
                            break;
                        }

                        PrintUsage();
                        break;
                    case "exit()":
                        return;
                    default:
                        PrintUsage();
                        break;
                }
            }
        }
    }

    private static void PrintUsage()
    {
        MyAnsiConsole.MarkupWarningLine("usage:");
        MyAnsiConsole.MarkupWarningLine("a*: search all keys start with a in db");
        MyAnsiConsole.MarkupWarningLine("del a2*: delete all keys start with a2 in db");
        MyAnsiConsole.MarkupWarningLine("select 1: checkout db 1");
        MyAnsiConsole.MarkupWarningLine("exit(): just exit");
    }
}