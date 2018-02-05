using System;
using System.Diagnostics;
using Metrics.NET.Owin.Sample.Common;
using Metrics.Utils;
using Microsoft.Owin.Hosting;

namespace Metrics.NET.Owin.Sample
{
    public class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:1235/";

            using (var scheduler = new ActionScheduler())
            {
                using (WebApp.Start<Startup>(url))
                {
                    Console.WriteLine("Owin Running at {0}", url);
                    Console.WriteLine("Press any key to exit");
                    Process.Start($"{url}metrics/");

                    SampleMetrics.RunSomeRequests();

                    scheduler.Start(TimeSpan.FromMilliseconds(500), () =>
                    {
                        SetCounterSample.RunSomeRequests();
                        SetMeterSample.RunSomeRequests();
                        UserValueHistogramSample.RunSomeRequests();
                        UserValueTimerSample.RunSomeRequests();
                        SampleMetrics.RunSomeRequests();
                    });

                    HealthChecksSample.RegisterHealthChecks();

                    Console.ReadKey();
                }
            }
        }
    }
}
