using Cli.Commands.ken_ws;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cli.Commands.ken_tr
{
    internal class TracerouteBinder : BinderBase<TracerouteType>
    {
        private readonly Argument<Uri> _uri;

        public TracerouteBinder(Argument<Uri> uri)
        {
            _uri = uri;
        }

        protected override TracerouteType GetBoundValue(BindingContext bindingContext) =>
            new()
            {
                WebSocketUri = bindingContext.ParseResult.GetValueForArgument(_uri)
            };
    }
}
