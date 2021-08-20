using System.CommandLine.Rendering;

namespace Cli.Extensions
{
    internal static class Colorizer
    {
        public static TextSpan Underline(this string value)
        {
            return new ContainerSpan(StyleSpan.UnderlinedOn(),
                                     new ContentSpan(value),
                                     StyleSpan.UnderlinedOff());
        }

        public static TextSpan Rgb(this string value, byte r, byte g, byte b)
        {
            return new ContainerSpan(ForegroundColorSpan.Rgb(r, g, b),
                                     new ContentSpan(value),
                                     ForegroundColorSpan.Reset());
        }

        public static TextSpan Color(this string text, ForegroundColorSpan foregroundColorSpan)
        {
            return new ContainerSpan(foregroundColorSpan,
                                     new ContentSpan(text),
                                     ForegroundColorSpan.Reset());
        }
    }
}
