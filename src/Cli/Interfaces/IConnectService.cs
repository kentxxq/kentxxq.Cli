using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Cli.Interfaces
{
    interface IConnectService
    {
        PingReply Ping(string url,int ttl=255);
    }
}
