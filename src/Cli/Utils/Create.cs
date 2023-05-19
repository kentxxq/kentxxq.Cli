using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cli.Utils;

public class Create
{
    /// <summary>
    /// 递归创建文件夹
    /// </summary>
    /// <param name="directory"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void CreateDirectory(string directory)
    {
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException("不能为空");
        }
        
        var directories = directory.Split(Path.DirectorySeparatorChar).SkipLast(1);
        var currentPath = "";
        foreach (var dir in directories)
        {
            currentPath = Path.Combine(currentPath, dir);

            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
            }
        }
    }

    /// <summary>
    /// 递归创建文件夹和文件
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="data"></param>
    /// <exception cref="ArgumentException"></exception>
    public static async Task CreateFile(string fullFilePath,string data = "")
    {
        if (string.IsNullOrEmpty(fullFilePath))
        {
            throw new ArgumentException("不能为空");
        }
        
        var directories = fullFilePath.Split(Path.DirectorySeparatorChar).SkipLast(1);
        var currentPath = "";
        foreach (var dir in directories)
        {
            currentPath = Path.Combine(currentPath, dir);

            if (!Directory.Exists(currentPath))
            {
                Directory.CreateDirectory(currentPath);
            }
        }

        await File.WriteAllTextAsync(fullFilePath, data);
    }
}