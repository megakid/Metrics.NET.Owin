using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.NET.Owin.Tests.Utils;
using Metrics.Owin;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;

namespace Metrics.NET.Owin.Tests
{
    public class OwinMiddlewareTests
    {
        private const int TimePerRequest = 100;

        private readonly TestContext _context = new TestContext();
        private readonly MetricsConfig _config;
        private readonly TestServer _server;

        public OwinMiddlewareTests()
        {
            _config = new MetricsConfig(_context);

            _server = TestServer.Create(app =>
            {
                _config.WithOwin(m => app.Use(m));

                app.Run(ctx =>
                {
                    _context.Clock.Advance(TimeUnit.Milliseconds, TimePerRequest);
                    if (ctx.Request.Path.ToString() == "/test/action")
                    {
                        return ctx.Response.WriteAsync("response");
                    }

                    if (ctx.Request.Path.ToString() == "/test/error")
                    {
                        ctx.Response.StatusCode = 500;
                        return ctx.Response.WriteAsync("response");
                    }

                    if (ctx.Request.Path.ToString() == "/test/size")
                    {
                        return ctx.Response.WriteAsync("response");
                    }

                    if (ctx.Request.Path.ToString() == "/test/post")
                    {
                        return ctx.Response.WriteAsync("response");
                    }

                    ctx.Response.StatusCode = 404;
                    return ctx.Response.WriteAsync("not found");
                });

            });
        }

        [Fact]
        public async Task OwinMetrics_ShouldBeAbleToRecordErrors()
        {
            _context.MeterValue("Owin", "Errors").Count.Should().Be(0);
            (await _server.HttpClient.GetAsync("http://local.test/test/error")).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            _context.MeterValue("Owin", "Errors").Count.Should().Be(1);

            (await _server.HttpClient.GetAsync("http://local.test/test/error")).StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            _context.MeterValue("Owin", "Errors").Count.Should().Be(2);
        }

        [Fact]
        public async Task OwinMetrics_ShouldBeAbleToRecordActiveRequestCounts()
        {
            _context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(0);
            (await _server.HttpClient.GetAsync("http://local.test/test/action")).StatusCode.Should().Be(HttpStatusCode.OK);
            _context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(1);
            (await _server.HttpClient.GetAsync("http://local.test/test/action")).StatusCode.Should().Be(HttpStatusCode.OK);
            _context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(2);
            (await _server.HttpClient.GetAsync("http://local.test/test/action")).StatusCode.Should().Be(HttpStatusCode.OK);
            _context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(3);
            (await _server.HttpClient.GetAsync("http://local.test/test/action")).StatusCode.Should().Be(HttpStatusCode.OK);
            _context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(4);

            var timer = _context.TimerValue("Owin", "Requests");

            timer.Histogram.Min.Should().Be(TimePerRequest);
            timer.Histogram.Max.Should().Be(TimePerRequest);
            timer.Histogram.Mean.Should().Be(TimePerRequest);
        }

        [Fact]
        public async Task OwinMetrics_ShouldRecordHistogramMetricsForPostSizeAndTimePerRequest()
        {
            const string json = "{ 'id': '1'} ";
            var postContent = new StringContent(json);
            postContent.Headers.Add("Content-Length", json.Length.ToString());
            await _server.HttpClient.PostAsync("http://local.test/test/post", postContent);

            var histogram = _context.HistogramValue("Owin", "Post & Put Request Size");

            histogram.Count.Should().Be(1);
            histogram.LastValue.Should().Be(json.Length);
        }
    }
}
