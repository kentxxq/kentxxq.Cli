using Cli.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cli.Commands.ken_tr
{
    internal class TracerouteType
    {
        public string HostName { get; set; } = null!;

        public IIpService IpService { get; set; } = null!;

        public IConnectService ConnectService { get; set; } = null!;
    }
}
