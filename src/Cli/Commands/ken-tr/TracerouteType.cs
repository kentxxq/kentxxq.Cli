using Cli.Interfaces;

namespace Cli.Commands.ken_tr
{
    internal class TracerouteType
    {
        public string HostName { get; set; } = null!;

        public IIpService IpService { get; set; } = null!;

        public IConnectService ConnectService { get; set; } = null!;
    }
}
