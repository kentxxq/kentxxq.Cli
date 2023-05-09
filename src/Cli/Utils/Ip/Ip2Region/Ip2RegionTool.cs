using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Cli.Utils.Ip.Ip2Region;

/// <summary>
/// Ip2Region工具
/// </summary>
public static class Ip2RegionTool
{
    /// <summary>
    /// 获取ip信息
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static async Task<IpServiceModel> GetIpInfo(string ip)
    {
        var httpClient = new HttpClient();
        var result = await httpClient.GetFromJsonAsync<IpServiceModel>($"https://test.kentxxq.com/ip/{ip}");
        return result ?? throw new ApplicationException("无法从test.kentxxq.com/ip获取ip信息");
    }
}