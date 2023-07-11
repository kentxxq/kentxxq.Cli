using Spectre.Console;

namespace Cli.Utils;

public static class MyAnsiConsole
{
    public static void MarkupError(string text)
    {
        AnsiConsole.Markup("[red]{0}[/]", Markup.Escape(text));
    }
    public static void MarkupErrorLine(string text)
    {
        AnsiConsole.MarkupLine("[red]{0}[/]", Markup.Escape(text));
    }
    
    public static void MarkupSuccess(string text)
    {
        AnsiConsole.Markup("[green]{0}[/]", Markup.Escape(text));
    }

    public static void MarkupSuccessLine(string text)
    {
        AnsiConsole.MarkupLine("[green]{0}[/]", Markup.Escape(text));
    }

    public static void MarkupWarning(string text)
    {
        AnsiConsole.Markup("[orange3]{0}[/]", Markup.Escape(text));
    }
    
    public static void MarkupWarningLine(string text)
    {
        AnsiConsole.MarkupLine("[orange3]{0}[/]", Markup.Escape(text));
    }
}