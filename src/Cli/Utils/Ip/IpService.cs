using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cli.Utils.Ip.Ip2Region;
using Cli.Utils.Ip.IpApi;

namespace Cli.Utils.Ip;

public static class IpService
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(3) };

    public static async Task<bool> ImInChina() => await InChina(await GetMyIP());

    public static async Task<bool> InChina(string ip)
    {
        var info = await GetIpInfo(ip);
        return info.Status == IpServiceQueryStatus.success &&
            new[] { "中国", "china", "cn" }.Contains(info.Country, StringComparer.OrdinalIgnoreCase);
    }

    public static async Task<string> GetMyIP()
    {
        foreach (var endpoint in new[] { ("https://uni.kentxxq.com/ip", "ip"), ("https://httpbin.org/ip", "origin") })
        {
            try
            {
                using var document = await Client.GetFromJsonAsync<JsonDocument>(endpoint.Item1);
                var text = document?.RootElement.GetProperty(endpoint.Item2).GetString()?.Split(',')[0].Trim();
                if (IPAddress.TryParse(text, out var ip)) return ip.ToString();
            }
            catch (Exception e) when (e is HttpRequestException or OperationCanceledException or JsonException or KeyNotFoundException or InvalidOperationException) { }
        }
        return "0.0.0.0";
    }

    public static async Task<IpServiceModel> GetIpInfo(string ip, CancellationToken ct = default)
    {
        foreach (var query in new Func<string, CancellationToken, Task<IpServiceModel>>[] { Ip2RegionTool.GetIpInfo, IpApiTool.GetIpInfo })
        {
            try
            {
                var result = await query(ip, ct);
                if (result.Status == IpServiceQueryStatus.success) return result;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception e) when (e is HttpRequestException or OperationCanceledException or JsonException or ApplicationException) { }
        }
        return new IpServiceModel { Status = IpServiceQueryStatus.fail, IP = ip, Country = "unknown", RegionName = "unknown", City = "unknown", Isp = "unknown" };
    }
}
