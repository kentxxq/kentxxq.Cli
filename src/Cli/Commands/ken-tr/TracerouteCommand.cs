using System;
using System.CommandLine;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Cli.Utils;
using Cli.Utils.Ip;
using Masuit.Tools;
using Spectre.Console;

namespace Cli.Commands.ken_tr;

internal static class TracerouteCommand
{
    private static readonly Argument<string> HostName = new("url", () => "kentxxq.com", "traceroute kentxxq.com");

    public static Command GetCommand()
    {
        var command = new Command("tr",
            @"(windows only.check https://github.com/dotnet/runtime/issues/927 for details)")
        {
            HostName
        };

        command.SetHandler(async context =>
        {
            var hostname = context.ParseResult.GetValueForArgument(HostName);

            // 测试连接
            TryConnect(hostname);

            await Run(hostname);
        });

        return command;
    }

    /// <summary>
    /// 尝试连接主机
    /// </summary>
    /// <param name="hostname"></param>
    private static void TryConnect(string hostname)
    {
        AnsiConsole.Markup($"try connecting to {hostname} ...");
        var reply = StaticPing.Ping(hostname);
        if (reply.Status == IPStatus.Success)
            MyAnsiConsole.MarkupSuccessLine("success");
        else
            MyAnsiConsole.MarkupWarningLine("failed");
    }

    private static async Task Run(string hostname)
    {
        var ttl = 1;
        // 目标主机的实际ip
        var hostIp = (await Dns.GetHostAddressesAsync(hostname))[0].ToString();
        var reply = StaticPing.Ping(hostname, ttl);

        // ttl为1的时候，可能会出现超时。会导致reply内的ip显示为目标主机ip。所以需要重新ping，拿到第二次的reply结果再进入循环
        if (reply.Status == IPStatus.TimedOut)
        {
            MyAnsiConsole.MarkupWarningLine($"{ttl} request timeout");
            ttl += 1;
            reply = StaticPing.Ping(hostname, ttl);
        }

        // 准备循环遍历中间网络节点
        while (ttl < 255 && reply.Address.ToString() != hostIp)
        {
            Console.Write($"{ttl} {reply.Address} take {reply.RoundtripTime}ms ");
            await PrintRegionByIp(reply.Address.ToString());

            ttl += 1;
            reply = StaticPing.Ping(hostname, ttl);
            // 如果超时，就输出timeout然后继续下个节点
            while (reply.Status == IPStatus.TimedOut)
            {
                MyAnsiConsole.MarkupWarningLine($"{ttl} request timeout");
                ttl += 1;
                reply = StaticPing.Ping(hostname, ttl);
            }
        }

        // 循环跳出的时候，不包含目标地址。所以需要多来一次
        Console.Write($"{ttl} {reply.Address} take {reply.RoundtripTime}ms ");
        await PrintRegionByIp(reply.Address.ToString());
    }

    /// <summary>
    /// 打印ip的地域信息
    /// </summary>
    /// <param name="ip"></param>
    private static async Task PrintRegionByIp(string ip)
    {
        if (ip.IsPrivateIP())
        {
            try
            {
                AnsiConsole.MarkupLine($"[green]{(await Dns.GetHostEntryAsync(ip)).HostName}[/]");
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[orange3]unknown host[/]");
            }
        }
        else
        {
            var ipAddress = (await Dns.GetHostAddressesAsync(ip))[0];
            var result = await IpService.GetIpInfo(ipAddress.ToString());
            MyAnsiConsole.MarkupSuccessLine($"{result.Country}-{result.RegionName}-{result.City}-{result.Isp}");
        }
    }
}