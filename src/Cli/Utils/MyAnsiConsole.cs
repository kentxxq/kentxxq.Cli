using Spectre.Console;

namespace Cli.Utils;

public static class MyAnsiConsole
{
    public static void MarkupErrorLine(string text)
    {
        AnsiConsole.MarkupLine($"[red]{text}[/]");
    }

    public static void MarkupSuccessLine(string? text)
    {
        AnsiConsole.MarkupLine($"[green]{text}[/]");
    }

    public static void MarkupWarningLine(string text)
    {
        AnsiConsole.MarkupLine($"[orange3]{text}[/]");
    }
}