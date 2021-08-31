using System.CommandLine.Rendering;

namespace Cli.Extensions
{
    public static class RenderText
    {
        public static void RenderError(this ConsoleRenderer consoleRenderer, string text)
        {
            consoleRenderer.RenderToRegion(text.Color(ForegroundColorSpan.Red()), Region.EntireTerminal);
            System.Console.WriteLine("");
        }

        public static void RenderSuccess(this ConsoleRenderer consoleRenderer, string text)
        {
            consoleRenderer.RenderToRegion(text.Color(ForegroundColorSpan.Green()), Region.EntireTerminal);
            System.Console.WriteLine("");
        }
    }
}
