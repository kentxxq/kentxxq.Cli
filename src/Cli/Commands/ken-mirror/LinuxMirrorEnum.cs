using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

namespace Cli.Commands.ken_mirror;

[EnumExtensions]
public enum LinuxMirrorEnum
{
    [Display(Name = "nothing")]
    Default,

    #region Ubuntu直接替换内容

    [Display(Name = "mirrors.aliyun.com")]
    AliyunUbuntu,
    [Display(Name = "mirrors.cloud.tencent.com")]
    TencentUbuntu,
    [Display(Name = "repo.huaweicloud.com")]
    HuaweiUbuntu,

    #endregion

    #region Centos下载新的配置文件

    [Display(Name = "https://mirrors.aliyun.com/repo/Centos-7.repo")]
    AliyunCentos7,
    [Display(Name = "https://mirrors.aliyun.com/repo/Centos-vault-8.5.2111.repo")]
    AliyunCentos8,
    [Display(Name = "http://mirrors.cloud.tencent.com/repo/centos7_base.repo")]
    TencentCentos7,
    [Display(Name = "http://mirrors.cloud.tencent.com/repo/centos8_base.repo")]
    TencentCentos8,
    [Display(Name = "https://repo.huaweicloud.com/repository/conf/CentOS-7-reg.repo")]
    HuaweiCentos7,
    [Display(Name = "https://repo.huaweicloud.com/repository/conf/CentOS-8-reg.repo")]
    HuaweiCentos8,

    #endregion
    
    
    
    
    
    
    [Display(Name = "http://mirrors.cloud.tencent.com/nexus/repository/maven-public/")]
    Tencent,
    [Display(Name = "https://repo.huaweicloud.com/repository/maven/")]
    Huawei
}