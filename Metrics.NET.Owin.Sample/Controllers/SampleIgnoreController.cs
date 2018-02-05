
using System.Web.Http;

namespace Metrics.NET.Owin.Sample.Controllers
{
    [RoutePrefix("sampleignore")]
    public class SampleIgnoreController : ApiController
    {
        [Route("")]
        public string Get()
        {
            return "get";
        }
    }
}
