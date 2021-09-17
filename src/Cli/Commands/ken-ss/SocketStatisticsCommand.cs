using Cli.Commands.ken_sp;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cli.Commands.ken_ss
{
    public class SocketStatisticsCommand
    {
        public static Command GetCommand()
        {
            var command = new Command("ss", "active tcp listening") {};

            command.Handler = CommandHandler.Create<IHost>(Run);
            return command;
        }

        private static void Run(IHost host)
        {
            IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            System.Net.IPEndPoint[] iPEndPoints = iPGlobalProperties.GetActiveTcpListeners();
            foreach (var item in iPEndPoints)
            {
                Console.WriteLine($"{item.Address}:{item.Port}");
            }
        }

    }
}
