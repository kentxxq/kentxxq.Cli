namespace Cli.Commands.ken_update.Proxy;

public class Normal : IProxyStrategy
{
    public string GetProxyUrl(string releaseUrl)
    {
        return releaseUrl;
    }
}