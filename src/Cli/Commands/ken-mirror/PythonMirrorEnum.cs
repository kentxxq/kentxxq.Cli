using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Cli.Commands.ken_mirror;

[EnumExtensions]
public enum PythonMirrorEnum
{
    [Display(Name = "https://pypi.org/simple/")]
    Default,
    [Display(Name = "https://mirrors.aliyun.com/pypi/simple/")]
    Aliyun,
    [Display(Name = "https://mirrors.cloud.tencent.com/pypi/simple")]
    Tencent,
    [Display(Name = "https://repo.huaweicloud.com/repository/pypi/simple")]
    Huawei
}