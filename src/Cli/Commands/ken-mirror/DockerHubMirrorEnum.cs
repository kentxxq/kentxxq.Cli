using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Cli.Commands.ken_mirror;

[EnumExtensions]
public enum DockerHubMirrorEnum
{
    [Display(Name = "https://hub-mirror.c.163.com")]
    NetEase163,
    [Display(Name = "https://dockerproxy.com")]
    DockerProxy,
    [Display(Name = "https://mirror.baidubce.com")]
    Baidu,
    [Display(Name = "")]
    Default
}