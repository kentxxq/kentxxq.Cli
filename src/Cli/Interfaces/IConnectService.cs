using System.Net.NetworkInformation;

namespace Cli.Interfaces;

internal interface IConnectService
{
    PingReply Ping(string url, int ttl = 255);
}