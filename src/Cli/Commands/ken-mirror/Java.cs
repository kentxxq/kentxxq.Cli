using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Cli.Utils;

namespace Cli.Commands.ken_mirror;

public static class Java
{
    private static readonly Option<JavaMirrorEnum> JavaMirror = new("--mirror", "-m") { Description = $"default {JavaMirrorEnum.Aliyun} mirror: {JavaMirrorEnum.Aliyun.ToStringFast(useMetadataAttributes: true)}", DefaultValueFactory = _ => JavaMirrorEnum.Aliyun };

    private static readonly string ConfigPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".m2{Path.DirectorySeparatorChar}settings.xml");
    
    public static Command GetCommand()
    {
        var command = new Command("java", "set maven mirror")
        {
            JavaMirror
        };
        command.SetAction(async (context, cancellationToken) =>
        {
            var javaMirror = context.GetValue(JavaMirror);
            if (!Enum.IsDefined(javaMirror)) throw new ArgumentException("镜像选项无效。");
            MyAnsiConsole.MarkupSuccessLine($"使用的maven源为 :{javaMirror}");
            await SetJavaMirror(javaMirror); 
        });
        return command;
    }

    private static async Task SetJavaMirror(JavaMirrorEnum mirror)
    {
        await WriteMirror(ConfigPath, mirror);
        MyAnsiConsole.MarkupSuccessLine("Maven 镜像配置已保存。");
    }

    internal static async Task WriteMirror(string path, JavaMirrorEnum mirror)
    {
        XNamespace ns = "http://maven.apache.org/SETTINGS/1.2.0";
        var doc = File.Exists(path) ? XDocument.Load(path) : new XDocument(new XElement(ns + "settings"));
        var root = doc.Root ?? throw new InvalidDataException("Maven 配置为空。");
        if (root.Name.LocalName != "settings") throw new InvalidDataException("Maven 根节点无效。");
        ns = root.Name.Namespace;
        var mirrors = root.Element(ns + "mirrors");
        if (mirrors is null) { mirrors = new XElement(ns + "mirrors"); root.Add(mirrors); }
        mirrors.Elements(ns + "mirror").Where(e => (string?)e.Element(ns + "id") == "ken-mirror").Remove();
        mirrors.Add(new XElement(ns + "mirror", new XElement(ns + "id", "ken-mirror"),
            new XElement(ns + "mirrorOf", "*"), new XElement(ns + "name", mirror.ToString()),
            new XElement(ns + "url", mirror.ToStringFast(useMetadataAttributes: true))));
        await Create.WriteConfig(path, doc.ToString());
    }
}
