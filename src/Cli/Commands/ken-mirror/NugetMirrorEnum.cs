using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Cli.Commands.ken_mirror;

[EnumExtensions]
public enum NugetMirrorEnum
{
    [Display(Name = "https://api.nuget.org/v3/index.json")]
    Default,
    [Display(Name = "https://mirrors.cloud.tencent.com/nuget/")]
    Huawei,
    [Display(Name = "https://mirrors.cloud.tencent.com/nuget/")]
    Tencent
}