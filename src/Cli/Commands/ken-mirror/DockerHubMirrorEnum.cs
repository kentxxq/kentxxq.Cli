using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Cli.Commands.ken_mirror;

[EnumExtensions]
public enum DockerHubMirrorEnum
{
    // [Display(Name = "https://docker.mirrors.ustc.edu.cn")]
    // USTC,
    // [Display(Name = "https://hub-mirror.c.163.com")]
    // NetEase163,
    // [Display(Name = "https://reg-mirror.qiniu.com")]
    // QiNiu,
    [Display(Name = "https://1ocw3lst.mirror.aliyuncs.com")]
    Aliyun,
    [Display(Name = "https://dca13e947d0a49ae8ac57d9bb7644e03.mirror.swr.myhuaweicloud.com")]
    Huawei,
    [Display(Name = "https://mirror.ccs.tencentyun.com")]
    Tencent,
    [Display(Name = "")]
    Default
}