
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Metrics.Owin.Middleware
{
    public abstract class MetricMiddleware
    {
        private readonly Regex[] _ignorePatterns;

        protected MetricMiddleware(Regex[] ignorePatterns)
        {
            _ignorePatterns = ignorePatterns;
        }

        protected bool PerformMetric(IDictionary<string, object> environment)
        {
            if (_ignorePatterns == null)
            {
                return true;
            }

            var requestPath = environment["owin.RequestPath"] as string;

            if (string.IsNullOrWhiteSpace(requestPath)) return false;

            return !_ignorePatterns.Any(ignorePattern => ignorePattern.IsMatch(requestPath.TrimStart('/')));
        }
    }
}
