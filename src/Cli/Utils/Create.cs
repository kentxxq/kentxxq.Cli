namespace Cli.Utils;

public static class Create
{
    public static void CreateDirectory(string directory) => Directory.CreateDirectory(directory);

    public static async Task CreateFile(string fullFilePath, string data = "")
    {
        var path = Path.GetFullPath(fullFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, data);
    }

    // 完整写入后才替换配置，备份与原文件放在一起以便恢复。
    public static async Task WriteConfig(string path, string data)
    {
        path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".ken-" + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, data);
            if (File.Exists(path))
            {
                File.Copy(path, path + ".ken-backup-" + Guid.NewGuid().ToString("N"));
                if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(temporary, File.GetUnixFileMode(path));
            }
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
