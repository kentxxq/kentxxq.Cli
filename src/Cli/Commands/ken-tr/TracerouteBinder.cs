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

        protected override TracerouteType GetBoundValue(BindingContext bindingContext)
        {
            var hostname = bindingContext.ParseResult.GetValueForArgument(_hostName);
            if(hostname == null)
            {
                throw new ArgumentException("参数错误");
            }
            else
            {
                return new()
                {
                    HostName = hostname,
                    ConnectService = new ConnectService(),
                    IpService = new IpService(new HttpClient())
                };
            }
        }
    }
}
