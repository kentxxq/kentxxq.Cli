using System.CommandLine;
using System.CommandLine.Binding;
using System.Net.Http;
using Cli.Services;

namespace Cli.Commands.ken_tr;

internal class TracerouteBinder : BinderBase<TracerouteType>
{
    private readonly Argument<string> _hostName;

    public TracerouteBinder(Argument<string> hostName)
    {
        _hostName = hostName;
    }

    protected override TracerouteType GetBoundValue(BindingContext bindingContext)
    {
        var hostname = bindingContext.ParseResult.GetValueForArgument(_hostName);
        return new TracerouteType
        {
            HostName = hostname,
            ConnectService = new ConnectService(),
            IpService = new IpService(new HttpClient())
        };
    }
}