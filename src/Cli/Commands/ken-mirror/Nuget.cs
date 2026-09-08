using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public static class Nuget
{
    private static readonly Option<NugetMirrorEnum> NugetMirror = new("--mirror", "-m") { Description = "NuGet 源，默认 Huawei", DefaultValueFactory = _ => NugetMirrorEnum.Huawei };
    
    public static Command GetCommand()
    {
        var command = new Command("nuget", "set nuget mirror")
        {
            NugetMirror
        };
        command.SetAction(async (context, cancellationToken) =>
        {
            var nugetMirror = context.GetValue(NugetMirror);
            if (!Enum.IsDefined(nugetMirror)) throw new ArgumentException("镜像选项无效。");
            MyAnsiConsole.MarkupSuccessLine($"使用的nuget源为 :{nugetMirror}");
            await SetNugetMirror(nugetMirror);
        });
        return command;
    }

    private static async Task SetNugetMirror(NugetMirrorEnum mirror)
    {
        var path = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NuGet", "NuGet.Config")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "NuGet", "NuGet.Config");
        await WriteMirror(path, mirror);
        MyAnsiConsole.MarkupSuccessLine("NuGet 源已添加或更新，其他源及启用状态保持不变。");
    }

    internal static async Task WriteMirror(string path, NugetMirrorEnum mirror)
    {
        var doc = File.Exists(path) ? System.Xml.Linq.XDocument.Load(path) : new System.Xml.Linq.XDocument(new System.Xml.Linq.XElement("configuration"));
        var root = doc.Root ?? throw new InvalidDataException("NuGet 配置为空。");
        if (root.Name != "configuration") throw new InvalidDataException("NuGet 根节点无效。");
        var sources = root.Element("packageSources");
        if (sources is null) { sources = new System.Xml.Linq.XElement("packageSources"); root.Add(sources); }
        var entry = sources.Elements("add").FirstOrDefault(e => (string?)e.Attribute("key") == mirror.ToString());
        if (entry is null) { entry = new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", mirror.ToString())); sources.Add(entry); }
        entry.SetAttributeValue("value", mirror.ToStringFast(useMetadataAttributes: true));
        await Create.WriteConfig(path, doc.ToString());
    }
}
