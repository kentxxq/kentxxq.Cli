using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Cli.Interfaces;
using Cli.Models;

namespace Cli.Services
{
    class IpService : IIpService
    {
        public HttpClient _httpClient { get; }
        public IpService(HttpClient client)
        {
            client.BaseAddress = new Uri("https://ip.taobao.com/outGetIpInfo");
            _httpClient = client;
        }

        public async Task<IpInfo> GetIpInfoByIp(string ip)
        {
            var result = await _httpClient.GetFromJsonAsync<IpInfo>($"?accessKey=alibaba-inc&&ip={ip}");
            if (result?.Code != Enums.IpInfoCode.个人qps超出)
            {
                Thread.Sleep(1000);
                result = await _httpClient.GetFromJsonAsync<IpInfo>($"?accessKey=alibaba-inc&&ip={ip}");
            }
            return result ?? new IpInfo();
        }
    }
}
