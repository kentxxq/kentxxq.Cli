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
    private static readonly Argument<string> Url = new("url") { Description = "url: redis.com" };

    /// <summary>
    /// redis的端口号，默认6379
    /// </summary>
    private static readonly Option<int> Port = new("--serverPort", "-port") { Description = "default:6379", DefaultValueFactory = _ => 6379 };

    /// <summary>
    /// 连接的db，默认为0
    /// </summary>
    private static readonly Option<int> Db = new("--database", "-db") { Description = "default:0", DefaultValueFactory = _ => 0 };

    /// <summary>
    /// 连接redis时的密码，默认为空
    /// </summary>
    private static readonly Option<string> Password = new("--password", "-p") { Description = "default empty", DefaultValueFactory = _ => "" };

    public static Command GetCommand()
    {
        var command = new Command("redis", "redis")
        {
            Url,
            Port,
            Db,
            Password
        };

        command.SetAction(async (context, cancellationToken) =>
        {
            var url = context.GetValue(Url);
            var port = context.GetValue(Port);
            var db = context.GetValue(Db);
            var password = context.GetValue(Password) ?? "";
            var ct = cancellationToken;
            await Run(url!, port, db, password, ct);
        });

        return command;
    }


    private static async Task Run(string url, int port, int db, string password, CancellationToken ct)
    {
        if (port is < 1 or > 65535 || db < 0) throw new ArgumentException("端口或数据库编号无效。");
        using var redis = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
        {
            EndPoints = { { url, port } }, ConnectRetry = 2, ConnectTimeout = 3000, Password = password
        });
        var server = redis.GetServer(url, port);
        await redis.GetDatabase(db).PingAsync();
        MyAnsiConsole.MarkupSuccessLine($"连接成功，当前数据库 {db}");
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            Console.Write(">");
            var input = await Console.In.ReadLineAsync(ct);
            if (input is null || input.Trim() == "exit()") return;
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            if (parts[0] == "select")
            {
                if (parts.Length != 2 || !int.TryParse(parts[1], out var nextDb) || nextDb < 0)
                {
                    PrintUsage();
                    continue;
                }
                await redis.GetDatabase(nextDb).PingAsync();
                db = nextDb;
                MyAnsiConsole.MarkupSuccessLine($"当前数据库 {db}");
            }
            else if (parts[0] == "del" && parts.Length == 2)
            {
                long deleted = 0;
                var database = redis.GetDatabase(db);
                await foreach (var key in server.KeysAsync(db, parts[1]).WithCancellation(ct))
                {
                    if (await database.KeyDeleteAsync(key)) deleted++;
                }
                MyAnsiConsole.MarkupWarningLine($"已删除 {deleted} 个键，数据库 {db}");
            }
            else if (parts[0] == "copy" && parts.Length == 3 && int.TryParse(parts[1], out var sourceDb) && sourceDb >= 0)
            {
                var source = redis.GetDatabase(sourceDb);
                long copied = 0;
                await foreach (var key in server.KeysAsync(sourceDb, parts[2]).WithCancellation(ct))
                {
                    if (await source.KeyCopyAsync(key, key, db)) copied++;
                }
                MyAnsiConsole.MarkupSuccessLine($"已复制 {copied} 个键到数据库 {db}");
            }
            else if (parts.Length == 1 && parts[0] is not ("del" or "copy"))
            {
                long count = 0;
                await foreach (var key in server.KeysAsync(db, parts[0]).WithCancellation(ct)) count++;
                MyAnsiConsole.MarkupSuccessLine($"匹配 {count} 个键，数据库 {db}");
            }
            else PrintUsage();
        }
    }

    private static void PrintUsage() => MyAnsiConsole.MarkupWarningLine(
        "用法：键模式 | del 模式 | select 数据库编号 | copy 来源数据库 模式 | exit()");
}
