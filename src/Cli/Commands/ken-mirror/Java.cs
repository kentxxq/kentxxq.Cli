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
    private static readonly Option<JavaMirrorEnum> JavaMirror = new(
        new[] { "-m", "--mirror" },
        ()=>JavaMirrorEnum.Aliyun,
        $"default {JavaMirrorEnum.Aliyun} mirror: {JavaMirrorEnum.Aliyun.ToStringFast()}"
    );

    private static readonly string ConfigPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".m2{Path.DirectorySeparatorChar}settings.xml");
    
    public static Command GetCommand()
    {
        var command = new Command("java", "set maven mirror")
        {
            JavaMirror
        };
        command.SetHandler(async context =>
        {
            var javaMirror = context.ParseResult.GetValueForOption(JavaMirror);
            MyAnsiConsole.MarkupSuccessLine($"使用的maven源为 :{javaMirror}");
            await SetJavaMirror(javaMirror); 
        });
        return command;
    }

    private static async Task SetJavaMirror(JavaMirrorEnum javaMirrorEnum)
    {
        MyLog.Logger?.Debug("配置文件路径:{ConfigPath}", ConfigPath);
        if (!File.Exists(ConfigPath))
        {
            MyAnsiConsole.MarkupSuccessLine($"配置文件不存在，新建中：{ConfigPath}");
            await Create.CreateFile(ConfigPath, """
                <?xml version="1.0" encoding="UTF-8"?>
                <settings xmlns="http://maven.apache.org/SETTINGS/1.2.0"
                  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://maven.apache.org/SETTINGS/1.2.0 https://maven.apache.org/xsd/settings-1.2.0.xsd">
                  <pluginGroups>
                  </pluginGroups>
                  <proxies>
                  </proxies>
                  <servers>
                  </servers>
                  <mirrors>
                    <!-- mirror
                     | Specifies a repository mirror site to use instead of a given repository. The repository that
                     | this mirror serves has an ID that matches the mirrorOf element of this mirror. IDs are used
                     | for inheritance and direct lookup purposes, and must be unique across the set of mirrors.
                     |
                    <mirror>
                      <id>mirrorId</id>
                      <mirrorOf>repositoryId</mirrorOf>
                      <name>Human Readable Name for this Mirror.</name>
                      <url>http://my.repository.com/repo/path</url>
                    </mirror>
                     -->
                  </mirrors>
                  <profiles>
                  </profiles>
                </settings>
                """);
        }
        

        var doc = new XmlDocument();
        doc.Load(ConfigPath);

        var mirrorsNode = doc.DocumentElement?["mirrors"];

        // 删掉已存在的ken-mirror,原来的mirror不会修改
        if (mirrorsNode?.ChildNodes != null)
        {
            MyAnsiConsole.MarkupWarningLine("检测到之前配置的ken-mirror，清理中...");
            foreach (XmlNode node in mirrorsNode.ChildNodes)
            {
                if (node["id"]?.InnerText == "ken-mirror")
                {
                    mirrorsNode.RemoveChild(node);
                }
            }
        }

        // 开始创建mirror节点
        var mirrorElement = doc.CreateElement("mirror",doc.DocumentElement?.NamespaceURI);
        var idElement = doc.CreateElement("id",doc.DocumentElement?.NamespaceURI);
        idElement.InnerText = "ken-mirror";
        var mirrorOfElement = doc.CreateElement("mirrorOf",doc.DocumentElement?.NamespaceURI);
        mirrorOfElement.InnerText = "*";
        var nameElement = doc.CreateElement("name",doc.DocumentElement?.NamespaceURI);
        nameElement.InnerText = javaMirrorEnum.ToString();
        var urlElement = doc.CreateElement("url",doc.DocumentElement?.NamespaceURI);
        urlElement.InnerText = javaMirrorEnum.ToStringFast();
        
        mirrorElement.AppendChild(idElement);
        mirrorElement.AppendChild(mirrorOfElement);
        mirrorElement.AppendChild(nameElement);
        mirrorElement.AppendChild(urlElement);
        
        mirrorsNode?.AppendChild(mirrorElement);
        
        doc.Save(ConfigPath);
        MyAnsiConsole.MarkupSuccessLine($"成功添加ken-mirror源,验证查看: {ConfigPath}");
    }
}