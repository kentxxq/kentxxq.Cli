using System;
using System.Linq;
using Serilog;

namespace Cli.Utils;

public class MyLog
{
    private const string LogTemplate = "{Timestamp:HH:mm:ss}|{Message:lj}{NewLine}";

    private static readonly Lazy<ILogger?> Lazy = new(() =>
    {
#if DEBUG
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: LogTemplate)
            .CreateLogger();
#else 
        if (Environment.GetCommandLineArgs().Contains("--debug"))
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: LogTemplate)
                .CreateLogger();
        }
        return null;
#endif
    });

    public static ILogger? Logger => Lazy.Value;
}