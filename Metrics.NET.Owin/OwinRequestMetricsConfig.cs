using System;
using System.Text.RegularExpressions;
using Metrics.Owin.Middleware;

namespace Metrics.Owin
{
    public class OwinRequestMetricsConfig
    {
        private readonly MetricsContext _metricsContext;
        private readonly Action<object> _middlewareRegistration;
        private readonly Regex[] _ignoreRequestPathPatterns;

        public OwinRequestMetricsConfig(Action<object> middlewareRegistration, MetricsContext metricsContext, Regex[] ignoreRequestPathPatterns)
        {
            _middlewareRegistration = middlewareRegistration;
            _metricsContext = metricsContext;
            _ignoreRequestPathPatterns = ignoreRequestPathPatterns;
        }

        /// <summary>
        /// Configure global OWIN Metrics.
        /// Available global metrics are: Request Timer, Active Requests Counter, Error Meter
        /// </summary>
        /// <returns>
        /// This instance to allow chaining of the configuration.
        /// </returns>
        public OwinRequestMetricsConfig WithAllOwinMetrics()
        {
            WithRequestTimer();
            WithActiveRequestCounter();
            WithPostAndPutRequestSizeHistogram();
            WithTimerForEachRequest();
            WithErrorsMeter();
            return this;
        }

        /// <summary>
        /// Registers a Timer metric named "Owin.Requests" that records how many requests per second are handled and also
        /// keeps a histogram of the request duration.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithRequestTimer(string metricName = "Requests")
        {
            var metricsMiddleware = new RequestTimerMiddleware(_metricsContext, metricName, _ignoreRequestPathPatterns);
            _middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a Counter metric named "Owin.ActiveRequests" that shows the current number of active requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithActiveRequestCounter(string metricName = "Active Requests")
        {
            var metricsMiddleware = new ActiveRequestCounterMiddleware(_metricsContext, metricName, _ignoreRequestPathPatterns);
            _middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Register a Histogram metric named "Owin.PostAndPutRequestsSize" on the size of the POST and PUT requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithPostAndPutRequestSizeHistogram(string metricName = "Post & Put Request Size")
        {
            var metricsMiddleware = new PostAndPutRequestSizeHistogramMiddleware(_metricsContext, metricName, _ignoreRequestPathPatterns);
            _middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a timer for each request.
        /// Timer is created based on route and will be named:
        /// Owin.{HTTP_METHOD_NAME} [{ROUTE_PATH}]
        /// </summary>
        public OwinRequestMetricsConfig WithTimerForEachRequest()
        {
            var metricsMiddleware = new TimerForEachRequestMiddleware(_metricsContext, _ignoreRequestPathPatterns);
            _middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a Meter metric named "Owin.Errors" that records the rate at which unhanded errors occurred while 
        /// processing Nancy requests.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithErrorsMeter(string metricName = "Errors")
        {
            var metricsMiddleware = new ErrorMeterMiddleware(_metricsContext, metricName, _ignoreRequestPathPatterns);
            _middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a Meter metric named "Owin.HttpStatusCodes" that records the rate at which given HTTP stats codes 
        /// are returned.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithHttpStatusCodeMeter(string metricName = "HttpStatusCodes")
        {
            var metricsMiddleware = new HttpStatusCodeMeterMiddleware(_metricsContext, metricName, _ignoreRequestPathPatterns);
            _middlewareRegistration(metricsMiddleware);
            return this;
        }
    }
}