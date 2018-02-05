using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metrics.Owin.Middleware
{
    /// <summary>
    /// Owin middleware that counts the number of active requests.
    /// </summary>
    public class ActiveRequestCounterMiddleware : MetricMiddleware
    {
        private readonly Counter _activeRequests;
        private Func<IDictionary<string, object>, Task> _next;

        public ActiveRequestCounterMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            _activeRequests = context.Counter(metricName, Unit.Custom("ActiveRequests"));
        }

        public void Initialize(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                _activeRequests.Increment();

                await _next(environment);

                _activeRequests.Decrement();
            }
            else
            {
                await _next(environment);
            }
        }
    }
}
