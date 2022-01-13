using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cli.Utils
{
    public static class MyAnsiConsole
    {
        public static void MarkupErrorLine(string text)
        {
            AnsiConsole.MarkupLine($"[red]{text}[/]");
        }

        public static void MarkupSuccessLine(string text)
        {
            AnsiConsole.MarkupLine($"[green]{text}[/]");
        }

        public static void MarkupWarningLine(string text)
        {
            AnsiConsole.MarkupLine($"[orange3]{text}[/]");
        }
    }
}
