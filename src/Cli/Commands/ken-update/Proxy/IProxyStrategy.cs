namespace Cli.Commands.ken_update.Proxy;

public interface IProxyStrategy
{
    /// <summary>
    /// 返回代理的下载地址
    /// </summary>
    /// <param name="releaseUrl">文件地址</param>
    /// <returns></returns>
    string GetProxyUrl(string releaseUrl);
}