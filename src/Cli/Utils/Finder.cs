using System;
using System.IO;

namespace Cli.Utils;

public class Finder
{
    /// <summary>
    /// 在环境变量-PATH的所有路径查找指定命令
    /// </summary>
    /// <param name="cmdName">命令名称</param>
    /// <returns></returns>
    public static string? FindCommand(string cmdName)
    {
        string? npmPath = null;
        
        var path = Environment.GetEnvironmentVariable("PATH");
        MyLog.Logger?.Debug($"PATH环境变量:{path}");
        var paths = path!.Split(Path.PathSeparator);
        
        foreach (var p in paths)
        {
            var filePath = Path.Combine(p, cmdName);

            if (File.Exists(filePath))
            {
                npmPath = filePath;
                MyLog.Logger?.Debug($"找到npm变量:{npmPath}");
                break;
            }
        }

        return npmPath;
    }
}