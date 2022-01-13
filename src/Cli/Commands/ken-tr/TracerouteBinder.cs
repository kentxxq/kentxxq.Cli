using Cli.Commands.ken_ws;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cli.Interfaces;
using Cli.Services;
using System.Net.Http;

namespace Cli.Commands.ken_tr
{
    internal class TracerouteBinder : BinderBase<TracerouteType>
    {
        private readonly Argument<string> _hostName;

        public TracerouteBinder(Argument<string> hostName)
        {
            _hostName = hostName;
        }

        protected override TracerouteType GetBoundValue(BindingContext bindingContext) =>
            new()
            {
                HostName = bindingContext.ParseResult.GetValueForArgument(_hostName),
                connectService = new ConnectService(),
                ipService = new IpService(new HttpClient())
            };
    }
}
