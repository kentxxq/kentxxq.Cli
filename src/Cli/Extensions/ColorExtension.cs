namespace Cli.Extensions;

public static class ColorExtension
{
    public static string NetworkDelayWithColor(this long time)
    {
        return time switch
        {
            < 100 => $"[green]{time}[/]",
            < 300 => $"[orange3]{time}[/]",
            _ => $"[red]{time}[/]"
        };
    }
}