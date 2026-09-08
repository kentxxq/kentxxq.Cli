namespace Cli.Utils;

public static class Finder
{
    public static string? FindCommand(string command)
    {
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var path = Path.Combine(directory.Trim('"'), command);
            if (File.Exists(path)) return path;
        }
        return null;
    }
}
