using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IDiagWeb.Controllers
{
    public class TestingController : ApiController
    {
        // GET: api/Testing/5
        public async Task<string> Get(int id)
        {
            var url = id == 1 
                ? "http://www.elastic.co" 
                : $"http://{ControllerContext.Request.RequestUri.Host}:{ControllerContext.Request.RequestUri.Port}/api/testing";

            using (var httpClient = new HttpClient())
            {
                
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
                var content = await response.Content.ReadAsStringAsync();
                return $"Downloaded {content.Length} from {url}";
            }
        }


        public async Task<IEnumerable<string>> Get()
        {
            await Task.CompletedTask;
            return new[]
            {
                "value 1",
                "value 2",
                "value 3"
            };
        }
    }
}
