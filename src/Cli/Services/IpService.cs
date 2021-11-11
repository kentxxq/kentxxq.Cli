using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Cli.Interfaces;
using Cli.Models;

namespace Cli.Services
{
    class IpService : IIpService
    {
        public HttpClient httpClient { get; }
        public IpService(HttpClient client)
        {
            client.BaseAddress = new Uri("https://ip.taobao.com/outGetIpInfo");
            httpClient = client;
        }

        public async Task<IpInfo> GetIpInfoByIp(string ip)
        {
            var result = await httpClient.GetFromJsonAsync<IpInfo>($"?accessKey=alibaba-inc&&ip={ip}", IpInfoContext.Default.IpInfo);
            if (result?.Code != Enums.IpInfoCode.个人qps超出)
            {
                Thread.Sleep(1000);
                result = await httpClient.GetFromJsonAsync<IpInfo>($"?accessKey=alibaba-inc&&ip={ip}", IpInfoContext.Default.IpInfo);
            }
            return result ?? new IpInfo();
        }
    }
}
