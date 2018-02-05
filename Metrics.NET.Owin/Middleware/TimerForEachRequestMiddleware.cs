using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics.Utils;

namespace Metrics.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class TimerForEachRequestMiddleware : MetricMiddleware
    {
        private const string RequestStartTimeKey = "__Metrics.RequestStartTime__";

        private readonly MetricsContext _context;

        private AppFunc _next;

        public TimerForEachRequestMiddleware(MetricsContext context, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            _context = context;
        }

        public void Initialize(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                environment[RequestStartTimeKey] = Clock.Default.Nanoseconds;

                await _next(environment);

                var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());
                var metricName = environment["owin.RequestPath"].ToString();

                if (environment.ContainsKey("metrics-net.routetemplate"))
                {
                    var requestMethod = environment["owin.RequestMethod"] as string;
                    var routeTemplate = environment["metrics-net.routetemplate"] as string;

                    metricName = requestMethod.ToUpperInvariant() + " " + routeTemplate;
                }

                if (httpResponseStatusCode != (int)HttpStatusCode.NotFound)
                {
                    var startTime = (long)environment[RequestStartTimeKey];
                    var elapsed = Clock.Default.Nanoseconds - startTime;
                    _context.Timer(metricName, Unit.Requests)
                        .Record(elapsed, TimeUnit.Nanoseconds);
                }
            }
            else
            {
                await _next(environment);
            }
        }
    }
}
