using System.CommandLine;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public static class NodeJS
{
    private static readonly Option<NodeJSMirrorEnum> NpmMirror = new("--mirror", "-m") { DefaultValueFactory = _ => NodeJSMirrorEnum.NpmMirror };
    public static Command GetCommand()
    {
        var command = new Command("nodejs", "设置用户级 npm registry") { NpmMirror };
        command.SetAction(async (result, ct) =>
        {
            if (!Enum.IsDefined(result.GetValue(NpmMirror))) throw new ArgumentException("镜像选项无效。");
            var path = Environment.GetEnvironmentVariable("NPM_CONFIG_USERCONFIG") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".npmrc");
            var lines = File.Exists(path) ? (await File.ReadAllLinesAsync(path, ct)).ToList() : [];
            lines.RemoveAll(line => System.Text.RegularExpressions.Regex.IsMatch(line, @"^\s*registry\s*=", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
            lines.Add("registry=" + result.GetValue(NpmMirror).ToStringFast(useMetadataAttributes: true));
            await Create.WriteConfig(path, string.Join(Environment.NewLine, lines) + Environment.NewLine);
            MyAnsiConsole.MarkupSuccessLine("用户级 npm registry 已保存。");
        });
        return command;
    }
}
