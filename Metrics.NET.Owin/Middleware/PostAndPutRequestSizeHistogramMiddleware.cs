using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metrics.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class PostAndPutRequestSizeHistogramMiddleware : MetricMiddleware
    {
        private readonly Histogram _histogram;
        private AppFunc _next;

        public PostAndPutRequestSizeHistogramMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            _histogram = context.Histogram(metricName, Unit.Bytes);
        }

        public void Initialize(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                var httpMethod = environment["owin.RequestMethod"].ToString().ToUpper();

                if (httpMethod == "POST" || httpMethod == "PUT")
                {
                    var headers = (IDictionary<string, string[]>)environment["owin.RequestHeaders"];
                    if (headers != null && headers.ContainsKey("Content-Length"))
                    {
                        _histogram.Update(long.Parse(headers["Content-Length"].First()));
                    }
                }
            }

            await _next(environment);
        }
    }
}
