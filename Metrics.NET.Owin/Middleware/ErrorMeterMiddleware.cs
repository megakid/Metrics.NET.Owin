using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metrics.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ErrorMeterMiddleware : MetricMiddleware
    {
        private readonly Meter _errorMeter;
        private AppFunc _next;

        public ErrorMeterMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            _errorMeter = context.Meter(metricName, Unit.Errors);
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

                var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());

                if (httpResponseStatusCode == (int)HttpStatusCode.InternalServerError)
                {
                    _errorMeter.Mark();
                }
            }
            else
            {
                await _next(environment);
            }
        }
    }
}
