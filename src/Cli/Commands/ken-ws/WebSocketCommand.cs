using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Threading;
using Cli.Utils;

namespace Cli.Commands.ken_ws
{
    internal static class WebSocketCommand
    {
        public static Argument<string> wsUrl = new("wsUrl", "wsUrl: wss://echo.kenxxq.com/ws");

        public static Command GetCommand()
        {
            var command = new Command("ws");
            command.AddArgument(wsUrl);

            command.Handler = CommandHandler.Create(Run);
            return command;
        }

        private static void Run()
        {
            var console = new SystemConsole();
            var render = new ConsoleRenderer(console, OutputMode.Ansi, true);
            var region = RegionTools.Live;

            for (int i = 0; i < 100; i++)
            {
                render.RenderToRegion($"nihao:{i}", region);
                Thread.Sleep(50);
            }

            var input = System.Console.ReadLine();
            if (input is not null)
            {
                render.RenderToRegion(input, Region.EntireTerminal);
            }
        }
    }
}
