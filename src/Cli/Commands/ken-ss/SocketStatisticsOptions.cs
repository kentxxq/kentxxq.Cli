namespace Cli.Commands.ken_ss
{
    class SocketStatisticsOptions
    {
        public bool Ipv4 { get; set; }
        public bool Ipv6 { get; set; }
        public bool Tcp { get; set; }
        public bool Udp { get; set; }
        public bool Listening { get; set; }
        public bool Processes { get; set; }
        public bool Numeric { get; set; }
    }
}
