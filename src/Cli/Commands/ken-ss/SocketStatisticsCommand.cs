using System;
using System.CommandLine;
using System.Net.NetworkInformation;

namespace Cli.Commands.ken_ss;

public static class SocketStatisticsCommand
{
    public static Command GetCommand()
    {
        var command = new Command("ss", "active tcp listening");

        command.SetHandler(Run);
        return command;
    }

    private static void Run()
    {
        var iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var iPEndPoints = iPGlobalProperties.GetActiveTcpListeners();
        foreach (var item in iPEndPoints) Console.WriteLine($"{item.Address}:{item.Port}");
    }
}