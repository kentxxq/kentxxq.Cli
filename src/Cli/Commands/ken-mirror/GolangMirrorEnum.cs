using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Cli.Commands.ken_mirror;

[EnumExtensions]
public enum GolangMirrorEnum
{
    [Display(Name = "https://mirrors.aliyun.com/goproxy/")]
    Aliyun,
    [Display(Name = "https://goproxy.cn,direct")]
    QiNiu,
    [Display(Name = "https://proxy.golang.com.cn,direct")]
    GoProxy,
    [Display(Name = "https://repo.huaweicloud.com/repository/goproxy/")]
    Huawei,
    [Display(Name = "https://mirrors.cloud.tencent.com/go/")]
    Tencent,
    [Display(Name = "https://proxy.golang.org,direct")]
    Default
}