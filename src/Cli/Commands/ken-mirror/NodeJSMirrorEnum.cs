using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Cli.Commands.ken_mirror;

[EnumExtensions]
public enum NodeJSMirrorEnum
{
    [Display(Name = "https://registry.npmmirror.com")]
    NpmMirror,
    [Display(Name = "https://registry.npmjs.org")]
    Default
}