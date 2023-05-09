using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Cli.Utils;

public static class StaticPing
{
    /// <summary>
    /// ping IPAddress
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="ttl">最大跳转数</param>
    /// <param name="timeout">超时时间(毫秒)</param>
    /// <returns></returns>
    public static bool PingIp(IPAddress ip,int ttl=255,int timeout=1000)
    {
        var pingSender = new Ping();
        // 32字节数据
        const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var buffer = Encoding.ASCII.GetBytes(data);
        var pingOption = new PingOptions(ttl, true);
        var reply = pingSender.Send(ip, timeout, buffer, pingOption);
        return reply.Status == IPStatus.Success;
    }
    
    /// <summary>
    /// ping 特定主机或ip
    /// </summary>
    /// <param name="url">主机或ip地址</param>
    /// <param name="ttl">最大跳转数</param>
    /// <param name="timeout">超时时间(毫秒)</param>
    /// <returns></returns>
    public static PingReply Ping(string url, int ttl=255,int timeout=1000)
    {
        var ping = new Ping();
        PingOptions pingOptions = new()
        {
            DontFragment = true,
            Ttl = ttl
        };

        const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var buffer = Encoding.ASCII.GetBytes(data);
        var reply = ping.Send(url, timeout, buffer, pingOptions);

        return reply;
    }

    /// <summary>
    /// ping IPAddress异步测试
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="ttl">最大跳转数</param>
    /// <param name="timeout">超时时间(毫秒)</param>
    /// <returns></returns>
    public static async Task<bool> PingIpAsync(IPAddress ip,int ttl=255,int timeout=1000)
    {
        var pingSender = new Ping();
        // 32字节数据
        const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var buffer = Encoding.ASCII.GetBytes(data);
        var pingOption = new PingOptions(ttl, true);
        var reply = await pingSender.SendPingAsync(ip, timeout, buffer, pingOption);
        return reply.Status == IPStatus.Success;
    }
}