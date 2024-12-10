namespace Cli.Commands.ken_update.Proxy;

public class Ghproxy : IProxyStrategy
{
    private const string ServerUrl = "https://ghp.ci/";

    public string GetProxyUrl(string releaseUrl)
    {
        return ServerUrl + releaseUrl;
    }
}
