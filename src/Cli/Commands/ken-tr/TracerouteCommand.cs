using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Drawing.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Cli.Interfaces;
using Masuit.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cli.Commands.ken_tr
{
    class TracerouteCommand
    {
        private static readonly Argument<string> hostName =new("url", () => "kentxxq.com", "traceroute kentxxq.com");

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
            var ip = tracerouteType.HostName;
            var url = tracerouteType.HostName.ToString();

            var reply = tracerouteType.connectService.Ping(url);
            switch (reply.Status)
            {
                case IPStatus.Success:
                    Console.WriteLine("connect success");
                    break;
                default:
                    Console.WriteLine("connect faild");
                    break;
            }

            var ttl = 1;
            reply = tracerouteType.connectService.Ping(url, ttl);
            reply = tracerouteType.connectService.Ping(reply.Address.ToString(), 255);
            while (ttl < 255 && reply.Address.ToString() != Dns.GetHostAddresses(url)[0].ToString())
            {
                Console.Write(ttl.ToString() + " " + reply.Address.ToString() + " ");
                Console.Write($"take {reply.RoundtripTime}ms" + " ");
                if (reply.Address.ToString().IsPrivateIP())
                {
                    try
                    {
                        Console.WriteLine(Dns.GetHostEntry(reply.Address).HostName);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("未知的主机");
                    }
                }
                else
                {
                    var result = await tracerouteType.ipService.GetIpInfoByIp(Dns.GetHostAddresses(reply.Address.ToString())[0].ToString());
                    Console.WriteLine(result?.ToString());
                }
                ttl += 1;
                reply = tracerouteType.connectService.Ping(url, ttl);
                while (reply.Status == IPStatus.TimedOut)
                {
                    Console.WriteLine(ttl.ToString() + " " + "无响应");
                    ttl += 1;
                    reply = tracerouteType.connectService.Ping(reply.Address.ToString(), 255);
                    reply = tracerouteType.connectService.Ping(url, ttl);
                }
                reply = tracerouteType.connectService.Ping(reply.Address.ToString(), 255);
            }
            Console.Write($"{ttl} {reply.Address} ");
            if (reply.Address.ToString().IsPrivateIP())
            {
                try
                {
                    Console.Write(Dns.GetHostEntry(reply.Address).HostName);
                }
                catch (Exception)
                {
                    Console.Write("未知的主机");
                }
            }
            else
            {
                var result = await tracerouteType.ipService.GetIpInfoByIp(Dns.GetHostAddresses(reply.Address.ToString())[0].ToString());
                Console.Write(result?.ToString());
            }
        }
    }
}
