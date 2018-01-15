using System.Collections.Generic;
using Metrics.Endpoints;

namespace Metrics.Owin.Middleware
{
    internal class OwinMetricsEndpointHandler : AbstractMetricsEndpointHandler<IDictionary<string, object>>
    {
        public OwinMetricsEndpointHandler(IEnumerable<MetricsEndpoint> endpoints) : base(endpoints) { }

        protected override MetricsEndpointRequest CreateRequest(IDictionary<string, object> requestInfo)
        {
            var owinRequestHeaders = requestInfo["owin.RequestHeaders"] as IDictionary<string, string[]>;
            return new MetricsEndpointRequest(owinRequestHeaders);
        }
    }
}
