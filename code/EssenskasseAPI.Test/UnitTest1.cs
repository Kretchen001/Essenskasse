using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace EssenskasseAPI.Test {

    public class PingTest {

        [Fact]
        public async Task PingOk() {
            await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>();
            using (HttpClient client = application.CreateClient()) {
                Assert.Equal(HttpStatusCode.OK, client.GetAsync("api/Ping/ping").Result.StatusCode);
            }
        }
    }
}