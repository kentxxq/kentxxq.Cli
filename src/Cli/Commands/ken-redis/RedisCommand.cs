using Spectre.Console;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cli.Commands.ken_redis
{
    public class RedisCommand
    {
        private static readonly Argument<string> url = new Argument<string>("url","url: redis.com");

        private static readonly Option<int> port = new(new[] { "-port", "--serverPort" }, () => 6379, "default:6379");

        private static readonly Option<int> db = new(new[] { "-db", "--database" }, () => 0, "default:0"); 

        private static readonly Option<string> password = new(new[] { "-p", "--password" }, () => "", "default empty");
        public static Command GetCommand()
        {
            var command = new Command("redis", "redis")
            {
                url,
                port,
                db,
                password
            };
            command.SetHandler<string,int,int,string>(Run,url,port,db,password);
            return command;
        }


        public static async Task<string> Run(string url,int port,int db,string password)
        {
            var redis = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
            {
                EndPoints = { $"bwd.kentxxq.com:6379" }
            });
            var dbc = redis.GetDatabase(db);
            var pong = await dbc.PingAsync();
            AnsiConsole.MarkupLine($"connect [green]success[/],take {pong.TotalMilliseconds} ms");
            
            var server = redis.GetServer(url, port);
            var keysCount = server.Keys(db, "*").Count();
            Console.WriteLine($"db的key数量:{keysCount}");
            Console.Write(">> ");


            await Task.Delay(1000);
            return "123";
        }
    }
}
