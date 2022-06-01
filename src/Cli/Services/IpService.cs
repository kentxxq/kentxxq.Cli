using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Cli.Enums;
using Cli.Interfaces;
using Cli.Models;

namespace Cli.Services;

internal class IpService : IIpService
{
    public IpService(HttpClient httpclient)
    {
        httpclient.BaseAddress = new Uri("https://ip.taobao.com/outGetIpInfo");
        Client = httpclient;
    }

    private HttpClient Client { get; }

    public async Task<IpInfo> GetIpInfoByIp(string ip)
    {
        var result = await Client.GetFromJsonAsync($"?accessKey=alibaba-inc&&ip={ip}", IpInfoContext.Default.IpInfo);
        if (result?.Code != IpInfoCode.个人qps超出)
        {
            Thread.Sleep(1000);
            result = await Client.GetFromJsonAsync($"?accessKey=alibaba-inc&&ip={ip}", IpInfoContext.Default.IpInfo);
        }

        return result ?? new IpInfo();
    }
}