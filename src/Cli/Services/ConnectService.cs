using System.Net.NetworkInformation;
using System.Text;
using Cli.Interfaces;

namespace Cli.Services;

internal class ConnectService : IConnectService
{
    public PingReply Ping(string url, int ttl)
    {
        var ping = new Ping();
        PingOptions pingOptions = new()
        {
            DontFragment = true,
            Ttl = ttl
        };

        const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var buffer = Encoding.ASCII.GetBytes(data);
        const int timeout = 1000; // 毫秒
        var reply = ping.Send(url, timeout, buffer, pingOptions);

        return reply;
    }
}