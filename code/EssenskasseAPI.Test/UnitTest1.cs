using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace EssenskasseAPI.Test {

    public class PingTest {

        [Fact]
        public async Task PingOk() {
            await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>();
            using (HttpClient client = application.CreateClient()) {
                HttpResponseMessage httpResponse = await client.GetAsync("api/Ping/ping");
                Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            }
        }
    }
}