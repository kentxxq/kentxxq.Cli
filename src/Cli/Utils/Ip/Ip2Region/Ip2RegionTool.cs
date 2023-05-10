using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cli.Utils.Ip.Ip2Region;

/// <summary>
/// Ip2Region工具
/// </summary>
public class Ip2RegionTool
{
    private static readonly JsonSerializerOptions MyJsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// 获取ip信息
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static async Task<IpServiceModel> GetIpInfo(string ip)
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        var result =
            await httpClient.GetFromJsonAsync<IpServiceModel>($"https://test.kentxxq.com/ip/{ip}",
                MyJsonSerializerOptions);
        return result ?? throw new ApplicationException("无法从test.kentxxq.com/ip获取ip信息");
    }
}