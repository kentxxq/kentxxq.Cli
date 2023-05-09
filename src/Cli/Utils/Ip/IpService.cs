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
    private static readonly List<string> ChinaStrings = new() { "中国", "CHINA","china","CN", "cn" };
    
    /// <summary>
    /// 自己的ip是否在国内
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> ImInChina()
    {
        var myIP = await GetMyIP();
        if (myIP == "0.0.0.0")
        {
            // 拿不到ip，默认在国内吧....
            return true;
        }
        else
        {
            return await InChina(myIP);
        }
    }
    
    /// <summary>
    /// 特定ip是否在国内
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static async Task<bool> InChina(string ip)
    {
        var result = await GetIpInfo(ip);
        // Console.WriteLine(ip);
        // Console.WriteLine(result.Country);
        // Console.WriteLine(ChinaStrings.Contains(result.Country) && result.Status == "success");
        return ChinaStrings.Contains(result.Country) && result.Status == "success";
    }
    
    
    /// <summary>
    /// 拿到自己的ip地址。如果都报错，会变成0.0.0.0
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetMyIP()
    {
        var httpClient = new HttpClient();
        var data = string.Empty;
        try
        {
            var result = await httpClient.GetStreamAsync("https://httpbin.org/ip");
            var jsonDoc = await JsonDocument.ParseAsync(result);
            data = jsonDoc.RootElement.GetProperty("origin").GetString();
        }
        catch (Exception)
        {
            var result = await httpClient.GetStreamAsync("https://test.kentxxq.com/ip");
            var jsonDoc = await JsonDocument.ParseAsync(result);
            data = jsonDoc.RootElement.GetProperty("ip").GetString();
        }
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
            var result = await IpApiTool.GetIpInfo(ip);
            return result;
        }
        catch (HttpRequestException e)
        {
            // ip-api.com不通，使用test.kentxxq.com(实际用的ip2region-20230509)
            var result = await Ip2RegionTool.GetIpInfo(ip);
            return result;
        }
        catch (Exception e)
        {
            // test.kentxxq.com 也不通，说明我已经没有在维护了。。
            return new IpServiceModel
            {
                Status = "failed",
                IP = ip,
                Country = "unknown",
                RegionName = "unknown",
                City = "unknown",
                Isp = "unknown",
            };
        }
    }
}