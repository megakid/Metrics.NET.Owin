using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metrics.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RequestTimerMiddleware : MetricMiddleware
    {
        private const string RequestStartTimeKey = "__Mertics.RequestStartTime__";

        private readonly Timer _requestTimer;
        private AppFunc _next;

        public RequestTimerMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            _requestTimer = context.Timer(metricName, Unit.Requests);
        }

        public void Initialize(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                environment[RequestStartTimeKey] = _requestTimer.StartRecording();

                await _next(environment);

                var endTime = _requestTimer.EndRecording();
                var startTime = (long)environment[RequestStartTimeKey];
                _requestTimer.Record(endTime - startTime, TimeUnit.Nanoseconds);

                environment.Remove(RequestStartTimeKey);
            }
            else
            {
                await _next(environment);
            }

        }
    }
}
