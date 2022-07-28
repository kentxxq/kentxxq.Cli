namespace Cli.Commands.ken_update.Proxy;

public class Fastgit:IProxyStrategy
{
    // https://github.com/hunshcn/gh-proxy
    private const string ServerUrl = "https://gh.api.99988866.xyz/";
    
    public string GetProxyUrl(string releaseUrl)
    {
        return ServerUrl + releaseUrl;
    }
}