namespace Cli.Commands.ken_update.Proxy;

public class Github : IProxyStrategy
{
    public string GetProxyUrl(string releaseUrl)
    {
        return releaseUrl;
    }
}