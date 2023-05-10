namespace Cli.Commands.ken_update.Proxy;

public class Fastgit : IProxyStrategy
{
    // https://github.com/hunshcn/gh-proxy
    private const string ServerUrl = "https://ken.kentxxq.workers.dev/";

    public string GetProxyUrl(string releaseUrl)
    {
        return ServerUrl + releaseUrl;
    }
}