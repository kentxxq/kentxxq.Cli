using System;
using System.CommandLine;
using System.Linq;
using System.Threading;
using Spectre.Console;
using StackExchange.Redis;

namespace Cli.Commands.ken_redis;

public static class RedisCommand
{
    private static readonly Argument<string> Url = new("url", "url: redis.com");

    private static readonly Option<int> Port = new(new[] { "-port", "--serverPort" }, () => 6379, "default:6379");

    private static readonly Option<int> Db = new(new[] { "-db", "--database" }, () => 0, "default:0");

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
        command.SetHandler<string, int, int, string, CancellationToken>(Run, Url, Port, Db, Password);
        return command;
    }


    private static void Run(string url, int port, int db, string password, CancellationToken ct)
    {
        password = password.Replace(" ", "");
        try
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
            Console.WriteLine($"db{db} keys:{keysCount}");

            while (!ct.IsCancellationRequested)
            {
                Console.Write(">>");
                var input = Console.ReadLine() ?? "";
                var inputs = input.Split(" ", 2);
                if (inputs.Length == 1 && input != "" && input != "exit()")
                {
                    var keys = server.Keys(db, input);
                    AnsiConsole.MarkupLine($"[green]keys count:{keys.Count()}[/]");
                }
                else if (input == "")
                {
                    Console.WriteLine("usage:");
                    Console.WriteLine($"a*: search db{db} all keys start with a");
                    Console.WriteLine($"del a2*: delete db{db} all keys start with a2");
                    Console.WriteLine("exit(): just exit");
                }
                else
                {
                    switch (inputs[0])
                    {
                        case "del":
                            var delKeys = server.Keys(db, inputs[1]);
                            var delKeysCount = dbc.KeyDelete(delKeys.ToArray());
                            AnsiConsole.MarkupLine($"[red]deleted {delKeysCount} key(s)[/]");
                            break;
                        case "exit()":
                            return;
                        default:
                            Console.WriteLine("unknown command");
                            break;
                    }
                }
            }
        }
        catch (RedisConnectionException e)
        {
            AnsiConsole.MarkupLine($"connect [red]failed[/]: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}