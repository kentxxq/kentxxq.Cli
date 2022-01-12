using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Hosting;

namespace Cli.Commands.ken_ss
{
    public class SocketStatisticsCommand
    {
        public static Command GetCommand()
        {
            var command = new Command("ss", "active tcp listening") { };

            command.SetHandler(Run);
            return command;
        }

        private static void Run()
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
