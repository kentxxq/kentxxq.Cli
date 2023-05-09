using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cli.Utils.Ip.Ip2Region;
using Cli.Utils.Ip.IpApi;

namespace Cli.Utils.Ip;

/// <summary>
/// ip服务
/// </summary>
public static class IpService
{
    /// <summary>
    /// 获取ip信息
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