using System.Net.NetworkInformation;

namespace Cli.Interfaces
{
    interface IConnectService
    {
        PingReply Ping(string url, int ttl = 255);
    }
}
