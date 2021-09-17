using Cli.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Cli.Services
{
    class ConnectService : IConnectService
    {
        public PingReply Ping(string url,int ttl)
        {
            var ping = new Ping();
            PingOptions pingOptions = new() {
                DontFragment = true,
                Ttl = ttl
            };            

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 2;
            var reply = ping.Send(url, timeout, buffer, pingOptions);
            //Console.WriteLine("Address: {0}", reply.Address.ToString());
            //Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
            //Console.WriteLine("Time to live: {0}", reply.Options?.Ttl);
            //Console.WriteLine("Don't fragment: {0}", reply.Options?.DontFragment);
            //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);

            return reply;
        }
    }
}
