namespace Cli.Commands.ken_update.Proxy;

public class Ghproxy : IProxyStrategy
{
    private const string ServerUrl = "https://mirror.ghproxy.com/";

    public string GetProxyUrl(string releaseUrl)
    {
        return ServerUrl + releaseUrl;
    }
}
