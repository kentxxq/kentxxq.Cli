using Cli.Utils;
using Masuit.Tools;
using Spectre.Console;
using System;
using System.CommandLine;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Cli.Commands.ken_tr
{
    class TracerouteCommand
    {
        private static readonly Argument<string> hostName = new("url", () => "kentxxq.com", "traceroute kentxxq.com");

        public static Command GetCommand()
        {
            var command = new Command("tr", @"(windows only.check https://github.com/dotnet/runtime/issues/927 for details)")
            {
                hostName
            };

            command.SetHandler(async (TracerouteType tracerouteType) => { await Run(tracerouteType); }, new TracerouteBinder(hostName));
            return command;
        }

        private static async Task Run(TracerouteType tracerouteType)
        {
            var url = tracerouteType.HostName.ToString();

            // 尝试连接目标主机
            Console.Write($"try connecting to {url} ...");
            var reply = tracerouteType.ConnectService.Ping(url);

            for (int i = 0; i < 2; i++)
            {
                if (reply.Status == IPStatus.Success)
                {
                    AnsiConsole.MarkupLine("[green]success[/]");
                    break;
                }
                else
                {
                    reply = tracerouteType.ConnectService.Ping(url);
                    if (i == 1)
                    {
                        AnsiConsole.MarkupLine("[red]faild[/]");
                    }
                }
            }


            var ttl = 1;
            reply = tracerouteType.ConnectService.Ping(url, ttl);
            if (reply.Status == IPStatus.TimedOut)
            {
                MyAnsiConsole.MarkupWarningLine(ttl.ToString() + " " + "*" + " ttl=1 packet was dropped");
                ttl += 1;
                reply = tracerouteType.ConnectService.Ping(url, ttl);
            };

            reply = tracerouteType.ConnectService.Ping(reply.Address.ToString(), 255);
            while (ttl < 255 && reply.Address.ToString() != Dns.GetHostAddresses(url)[0].ToString())
            {
                Console.Write(ttl.ToString() + " " + reply.Address.ToString() + " ");
                Console.Write($"take {reply.RoundtripTime}ms" + " ");
                if (reply.Address.ToString().IsPrivateIP())
                {
                    try
                    {
                        AnsiConsole.MarkupLine($"[green]{Dns.GetHostEntry(reply.Address).HostName}[/]");
                    }
                    catch (Exception)
                    {
                        AnsiConsole.MarkupLine("[orange3]unknown host[/]");
                    }
                }
                else
                {
                    var result = await tracerouteType.IpService.GetIpInfoByIp(Dns.GetHostAddresses(reply.Address.ToString())[0].ToString());
                    MyAnsiConsole.MarkupSuccessLine(result?.ToString());
                }
                ttl += 1;
                reply = tracerouteType.ConnectService.Ping(url, ttl);
                while (reply.Status == IPStatus.TimedOut)
                {
                    MyAnsiConsole.MarkupWarningLine(ttl.ToString() + " " + "");
                    ttl += 1;
                    tracerouteType.ConnectService.Ping(reply.Address.ToString(), 255);
                    reply = tracerouteType.ConnectService.Ping(url, ttl);
                }
                reply = tracerouteType.ConnectService.Ping(reply.Address.ToString(), 255);
            }
            Console.Write($"{ttl} {reply.Address} ");
            if (reply.Address.ToString().IsPrivateIP())
            {
                try
                {
                    AnsiConsole.Markup($"[green]{Dns.GetHostEntry(reply.Address).HostName}[/]");
                }
                catch (Exception)
                {
                    AnsiConsole.Markup("[orange3]unknown host[/]");
                }
            }
            else
            {
                var result = await tracerouteType.IpService.GetIpInfoByIp(Dns.GetHostAddresses(reply.Address.ToString())[0].ToString());
                Console.Write(result?.ToString());
            }
        }
    }
}
