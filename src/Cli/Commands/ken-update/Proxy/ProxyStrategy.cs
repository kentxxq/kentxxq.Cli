using System;

namespace Cli.Commands.ken_update.Proxy;

public class ProxyStrategy
{
    private readonly IProxyStrategy _proxyStrategy;

    public ProxyStrategy(ProxyEnum proxy)
    {
        var t = Activator.CreateInstance("ken", "Cli.Commands.ken_update.Proxy." + proxy)?.Unwrap() as IProxyStrategy;
        _proxyStrategy = t ?? throw new ArgumentNullException(proxy.ToString(), "proxy strategy not found");
    }

    public string GetDownloadUrl(ProxyEnum proxy, string url)
    {
        return _proxyStrategy.GetProxyUrl(url);
    }
}