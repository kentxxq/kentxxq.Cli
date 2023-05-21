using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Cli.Commands.ken_mirror;

[EnumExtensions]
public enum JavaMirrorEnum
{
    [Display(Name = "https://repo.maven.apache.org/maven2/")]
    Default,
    [Display(Name = "https://maven.aliyun.com/repository/public")]
    Aliyun,
    [Display(Name = "http://mirrors.cloud.tencent.com/nexus/repository/maven-public/")]
    Tencent,
    [Display(Name = "https://repo.huaweicloud.com/repository/maven/")]
    Huawei
}