using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using k8s;
using Microsoft.IdentityModel.Tokens;

namespace Cli.Commands.ken_k8s;

public static class ConfigUtils
{
    public static async Task<KubernetesClientConfiguration> GetConfig(string? configPath)
    {
        if (configPath.IsNullOrEmpty())
            configPath = KubernetesClientConfiguration.KubeConfigDefaultLocation;
        else if (!Path.IsPathRooted(configPath))
            configPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? @"\", @$".kube\{configPath}")
                : Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? "/", $".kube/{configPath}");

        var config = await KubernetesClientConfiguration.BuildConfigFromConfigFileAsync(new FileInfo(configPath));
        return config;
    }
}