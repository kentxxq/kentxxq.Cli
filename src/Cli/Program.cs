using System.CommandLine;
using System.Threading.Tasks;
using Cli.Commands.ken_sp;
using Cli.Commands.ken_ws;

namespace Cli
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(SocketPingCommand.GetCommand());
            rootCommand.AddCommand(WebSocketCommand.GetCommand());
            //await rootCommand.InvokeAsync(new string[] { "sp", "baidu.com:443", "-n 3" });
#if DEBUG
            //await rootCommand.InvokeAsync(new string[] { "sp", "baidu.com:443", "-n 3" });
            await rootCommand.InvokeAsync(new string[] { "ws", "baidu.com:443" });
#else
            await rootCommand.InvokeAsync(args);
#endif
        }
    }
}
