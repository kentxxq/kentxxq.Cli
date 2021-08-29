using System;
using System.CommandLine.Rendering;

namespace Cli.Utils
{
    internal static class RegionTools
    {
        /// <summary>
        /// 在当前行进行刷新输出
        /// </summary>
        public static Region Live => new(0, Console.CursorTop, Console.WindowWidth, Console.WindowHeight, false);

    }
}
