﻿using System;
using System.Text.RegularExpressions;
using System.Web.Http;
using Metrics.Endpoints;
using Metrics.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;

namespace Metrics.NET.Owin.Sample
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            app.UseCors(CorsOptions.AllowAll);

            var httpconfig = new HttpConfiguration();
            httpconfig.MapHttpAttributeRoutes();

            // Sets the route template for the current request in the OWIN context
            httpconfig.MessageHandlers.Add(new SetOwinRouteTemplateMessageHandler());

            Metric.Config
                .WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(30)))
                .WithOwin(middleware => app.Use(middleware), config => config
                    .WithRequestMetricsConfig(c => c.WithAllOwinMetrics(), new[]
                    {
                        new Regex("(?i)^sampleignore"),
                        new Regex("(?i)^metrics"),
                        new Regex("(?i)^health"),
                        new Regex("(?i)^json")
                     })
                    .WithMetricsEndpoint(conf => conf
                        .WithEndpointReport("/test", (d, h, r) => new MetricsEndpointResponse("test", "text/plain")))
                );

            app.UseWebApi(httpconfig);
        }

    }
}
