using System.CommandLine;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Cli.Utils;
using Cli.Utils.Ip;
using Masuit.Tools;

namespace Cli.Commands.ken_tr;

internal static class TracerouteCommand
{
    private static readonly Argument<string> HostName = new("url") { DefaultValueFactory = _ => "kentxxq.com" };
    public static Command GetCommand()
    {
        var command = new Command("tr", "路由跟踪，仅支持 Windows IPv4") { HostName };
        command.SetAction(async (result, ct) =>
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("路由跟踪仅支持 Windows。");
            var addresses = await Dns.GetHostAddressesAsync(result.GetValue(HostName)!, ct);
            var target = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                ?? throw new ArgumentException("目标没有 IPv4 地址。");
            using var ping = new Ping();
            for (var ttl = 1; ttl <= 255; ttl++)
            {
                ct.ThrowIfCancellationRequested();
                var reply = await ping.SendPingAsync(target, TimeSpan.FromSeconds(1), new byte[32], new PingOptions(ttl, true), ct);
                if (reply.Status == IPStatus.TimedOut || reply.Address is null)
                {
                    MyAnsiConsole.MarkupWarningLine($"{ttl} 请求超时");
                    continue;
                }
                MyAnsiConsole.MarkupSuccess($"{ttl} {reply.Address} {reply.RoundtripTime}ms {reply.Status}");
                if (!IPAddress.IsLoopback(reply.Address) && !reply.Address.ToString().IsPrivateIP())
                {
                    var info = await IpService.GetIpInfo(reply.Address.ToString(), ct);
                    MyAnsiConsole.MarkupSuccess($" {info.Country}-{info.RegionName}-{info.Isp}");
                }
                Console.WriteLine();
                if (reply.Status == IPStatus.Success && reply.Address.Equals(target)) return 0;
                if (reply.Status != IPStatus.TtlExpired) return 1;
            }
            return 1;
        });
        return command;
    }
}
