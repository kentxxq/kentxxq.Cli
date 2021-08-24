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
#if DEBUG
            //await rootCommand.InvokeAsync(new string[] { "sp", "baidu.com:443", "-n 3" });
            //await rootCommand.InvokeAsync(new string[] { "ws", "wss://ws.kenxxq.com/ws" });
            await rootCommand.InvokeAsync(new string[] { "ws", "wss://ws.kentxxq.com/ws" });
#else
            await rootCommand.InvokeAsync(args);
#endif
        }
    }
}
