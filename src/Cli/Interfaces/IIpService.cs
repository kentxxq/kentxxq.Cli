using Cli.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
