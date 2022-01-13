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

        public IIpService ipService { get; set; } = null!;

        public IConnectService connectService { get; set; } = null!;
    }
}
