using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Cli.Utils.Ip.Ip2Region;
using Cli.Utils.Ip.IpApi;

namespace Cli.Utils.Ip;

/// <summary>
/// ip服务
/// </summary>
public static class IpService
{
    private static readonly List<string> ChinaStrings = new() { "中国", "CHINA", "china", "CN", "cn" };

    /// <summary>
    /// 自己的ip是否在国内
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> ImInChina()
    {
        var myIP = await GetMyIP();
        if (myIP == "0.0.0.0")
        {
            MyLog.Logger?.Debug("拿不到ip，默认在国内吧....");
            return true;
        }

        return await InChina(myIP);
    }

    /// <summary>
    /// 特定ip是否在国内
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static async Task<bool> InChina(string ip)
    {
        var result = await GetIpInfo(ip);
        MyLog.Logger?.Debug("在国内？{Contains}",
            ChinaStrings.Contains(result.Country) && result.Status == IpServiceQueryStatus.success);
        return ChinaStrings.Contains(result.Country) && result.Status == IpServiceQueryStatus.success;
    }


    /// <summary>
    /// 拿到自己的ip地址。如果都报错，会变成0.0.0.0
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetMyIP()
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        string? data;
        try
        {
            var result = await httpClient.GetStreamAsync("https://uni.kentxxq.com/ip");
            var jsonDoc = await JsonDocument.ParseAsync(result);
            data = jsonDoc.RootElement.GetProperty("ip").GetString();
        }
        catch (Exception)
        {
            var result = await httpClient.GetStreamAsync("https://httpbin.org/ip");
            var jsonDoc = await JsonDocument.ParseAsync(result);
            data = jsonDoc.RootElement.GetProperty("origin").GetString();
        }

        MyLog.Logger?.Debug("通过api的查询结果，我的ip是: {Data}", data);
        return string.IsNullOrEmpty(data) ? "0.0.0.0" : data;
    }


    /// <summary>
    /// 获取特定ip信息
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static async Task<IpServiceModel> GetIpInfo(string ip)
    {
        try
        {
            MyLog.Logger?.Debug("使用ip2region-20230509获取ip信息");
            var result = await Ip2RegionTool.GetIpInfo(ip);
            return result;
        }
        catch (HttpRequestException)
        {
            MyLog.Logger?.Debug("uni.kentxxq.com不通，使用ip-api.com");
            var result = await IpApiTool.GetIpInfo(ip);
            return result;
        }
        catch (Exception)
        {
            MyLog.Logger?.Debug("两个api都查询失败...说明我已经没有在维护了...");
            return new IpServiceModel
            {
                Status = IpServiceQueryStatus.fail,
                IP = ip,
                Country = "unknownCountry",
                RegionName = "unknownRegionName",
                City = "unknownCity",
                Isp = "unknownIsp"
            };
        }
    }
}
