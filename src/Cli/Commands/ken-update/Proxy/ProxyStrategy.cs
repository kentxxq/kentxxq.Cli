using System;

namespace Cli.Commands.ken_update;

public class ProxyStrategy
{
    private IProxyStrategy _proxyStrategy;
    public ProxyStrategy(ProxyEnum proxy)
    {
        var t = (IProxyStrategy)Activator.CreateInstance(null,"Cli.Commands.ken_update.Proxy."+proxy);
        _proxyStrategy = t;
    }

    public string GetDownloadUrl(ProxyEnum proxy,string url)
    {
        return _proxyStrategy.GetProxyUrl(url);
    }
}