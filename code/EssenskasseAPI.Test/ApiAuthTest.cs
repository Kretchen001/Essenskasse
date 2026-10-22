using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace EssenskasseAPI.Test {

    /// <summary>Prueft, dass die geschuetzten Endpunkte ohne gueltiges Sitzungs-Token abgewiesen werden.
    /// Diese Tests brauchen keine Datenbank: ohne (oder mit unlesbarem) Token wird noch vor dem Datenbankzugriff mit 401 geantwortet.</summary>
    public class ApiAuthTest : IClassFixture<WebApplicationFactory<Program>> {

        private readonly WebApplicationFactory<Program> _factory;

        public ApiAuthTest(WebApplicationFactory<Program> factory) {
            _factory = factory;
        }

        public static TheoryData<string, string> ProtectedEndpoints => new() {
            { "GET", "api/person/perGuid" },
            { "POST", "api/person/new" },
            { "PUT", "api/person/update" },
            { "GET", "api/person/requestHidden" },
            { "PUT", "api/person/restore" },
            { "PUT", "api/person/updateDatabasePersonSaldo" },
            { "DELETE", "api/person/remove" },
            { "GET", "api/order/requestAll" },
            { "POST", "api/order/insertNew" },
            { "PUT", "api/order/update" },
            { "DELETE", "api/order/remove" },
            { "GET", "api/payment/requestAll" },
            { "POST", "api/payment/insertNew" },
            { "PUT", "api/payment/update" },
            { "DELETE", "api/payment/remove" },
            { "GET", "api/provider/requestAll" },
            { "POST", "api/provider/new" },
            { "PUT", "api/provider/update" },
            { "DELETE", "api/provider/remove" },
            { "GET", "api/provider/statistics" },
            { "GET", "api/provider/testExist" },
            { "GET", "api/transactionprocedure/all" },
            { "POST", "api/transactionprocedure/new" },
            { "PUT", "api/user/update" },
            { "GET", "api/user/exist" },
            { "POST", "api/backup/newNow" },
            { "POST", "api/backup/restore" },
        };

        [Theory]
        [MemberData(nameof(ProtectedEndpoints))]
        public async Task ProtectedEndpoint_WithoutToken_Returns401(string method, string url) {
            using HttpClient client = _factory.CreateClient();
            using HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("Bearer kein-guid")]
        [InlineData("Bearer ")]
        [InlineData("Basic dGs6dGtr")]
        public async Task ProtectedEndpoint_WithMalformedToken_Returns401(string authorization) {
            using HttpClient client = _factory.CreateClient();
            HttpRequestMessage request = new(HttpMethod.Get, "api/order/requestAll");
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
            using HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Ping_IsPublic() {
            using HttpClient client = _factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("api/ping/ping");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_IsAvailableByDefault_AndDescribesBearerAuth() {
            using HttpClient client = _factory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync("swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string json = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"Bearer\"", json);
        }
    }
}
