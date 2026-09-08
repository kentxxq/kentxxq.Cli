using System.CommandLine;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public static class Python
{
    private static readonly Option<PythonMirrorEnum> PythonMirror = new("--mirror", "-m") { DefaultValueFactory = _ => PythonMirrorEnum.Aliyun };
    public static Command GetCommand()
    {
        var command = new Command("python", "设置用户级 pip index-url") { PythonMirror };
        command.SetAction(async (result, ct) =>
        {
            if (!Enum.IsDefined(result.GetValue(PythonMirror))) throw new ArgumentException("镜像选项无效。");
            var path = Environment.GetEnvironmentVariable("PIP_CONFIG_FILE");
            if (string.IsNullOrEmpty(path)) path = OperatingSystem.IsWindows()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pip", "pip.ini")
                : Path.Combine(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config"), "pip", "pip.conf");
            var lines = File.Exists(path) ? (await File.ReadAllLinesAsync(path, ct)).ToList() : [];
            var start = lines.FindIndex(line => line.Trim().Equals("[global]", StringComparison.OrdinalIgnoreCase));
            if (start < 0) { lines.Add("[global]"); start = lines.Count - 1; }
            var end = start + 1;
            while (end < lines.Count && !lines[end].TrimStart().StartsWith('[')) end++;
            for (var i = end - 1; i > start; i--)
                if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], @"^\s*index-url\s*=", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) lines.RemoveAt(i);
            lines.Insert(start + 1, "index-url = " + result.GetValue(PythonMirror).ToStringFast(useMetadataAttributes: true));
            await Create.WriteConfig(path, string.Join(Environment.NewLine, lines) + Environment.NewLine);
            MyAnsiConsole.MarkupSuccessLine("用户级 pip index-url 已保存；uv 不读取 pip 配置。");
        });
        return command;
    }
}
