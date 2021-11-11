using System.Threading.Tasks;
using Cli.Models;

namespace Cli.Interfaces
{
    interface IIpService
    {
        /// <summary>
        /// 获取ip的相关信息
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <returns></returns>
        Task<IpInfo> GetIpInfoByIp(string ip);
    }
}
