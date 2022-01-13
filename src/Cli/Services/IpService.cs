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
        private HttpClient Client { get; }
        public IpService(HttpClient httpclient)
        {
            httpclient.BaseAddress = new Uri("https://ip.taobao.com/outGetIpInfo");
            Client = httpclient;
        }

        public async Task<IpInfo> GetIpInfoByIp(string ip)
        {
            var result = await Client.GetFromJsonAsync<IpInfo>($"?accessKey=alibaba-inc&&ip={ip}", IpInfoContext.Default.IpInfo);
            if (result?.Code != Enums.IpInfoCode.个人qps超出)
            {
                Thread.Sleep(1000);
                result = await Client.GetFromJsonAsync<IpInfo>($"?accessKey=alibaba-inc&&ip={ip}", IpInfoContext.Default.IpInfo);
            }
            return result ?? new IpInfo();
        }
    }
}
