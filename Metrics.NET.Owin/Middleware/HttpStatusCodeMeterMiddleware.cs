using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metrics.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HttpStatusCodeMeterMiddleware : MetricMiddleware
    {
        private readonly Meter _httpStatusCodeMeter;
        private AppFunc _next;

        public HttpStatusCodeMeterMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            _httpStatusCodeMeter = context.Meter(metricName, Unit.Errors);
        }

        public void Initialize(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                await _next(environment);

                _httpStatusCodeMeter.Mark(environment["owin.ResponseStatusCode"].ToString());
            }
            else
            {
                await _next(environment);
            }
        }
    }
}
