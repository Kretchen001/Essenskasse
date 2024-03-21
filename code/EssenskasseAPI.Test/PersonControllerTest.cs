/// <summary>
/// ! AFTER MYSQL UPDATE NOT TESTED!!!
/// </summary>
namespace EssenskasseAPI.Test {

    // [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    // public class PersonControllerTest {

    //     /// <summary>That the test run on an cleared system, the database file should be deleted, if it was there.</summary>
    //     public PersonControllerTest() {
    //         if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/data.liteDB")) {
    //             File.Delete(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/data.liteDB");
    //         }
    //     }

    //     [Fact, Priority(0)]
    //     public async Task PersonGet() {
    //         await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>();
    //         using (HttpClient client = application.CreateClient()) {
    //             HttpResponseMessage response = await client.GetAsync("api/Person/requestAll");
    //             Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    //             Assert.Equal("", await response.Content.ReadAsStringAsync());
    //         }
    //     }

    //     [Fact, Priority(10)]
    //     public async Task PersonNotFoundGuid() {
    //         await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>();
    //         using (HttpClient client = application.CreateClient()) {
    //             HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/Person/perGuid") {
    //                 Headers = {
    //                     { "guid", new Guid("00000000-0000-0000-0000-000000000000").ToString() }
    //                 }
    //             };
    //             HttpResponseMessage response = await client.SendAsync(httpRequestMessage);
    //             Assert.True(response.StatusCode.Equals(HttpStatusCode.NotFound));
    //         }
    //     }

    //     [Fact, Priority(20)]
    //     public async Task PersonNew() {
    //         await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>();
    //         using (HttpClient client = application.CreateClient()) {
    //             HttpResponseMessage response = await client.PostAsync("api/Person/new", null);
    //             Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    //             HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/Person/new") {
    //                 Headers = {
    //                     {"name", Uri.EscapeDataString("Michael R.")},
    //                 }
    //             };
    //             HttpResponseMessage response2 = await client.SendAsync(httpRequestMessage);
    //             Guid guid = JsonConvert.DeserializeObject<Guid>(await response2.Content.ReadAsStringAsync());

    //             HttpRequestMessage httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "api/Person/perGuid") {
    //                 Headers = {
    //                     { "guid", guid.ToString() }
    //                 }
    //             };
    //             HttpResponseMessage response3 = await client.SendAsync(httpRequestMessage2);
    //             Assert.True(response3.IsSuccessStatusCode);
    //         }
    //     }

    //     [Fact, Priority(30)]
    //     public async Task PersonUpdate() {
    //         await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>();
    //         using (HttpClient client = application.CreateClient()) {
    //         }
    //     }

    //     [Fact, Priority(40)]
    //     public async Task PersonRequestAll() {
    //         await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>();
    //         using (HttpClient client = application.CreateClient()) {
    //         }
    //     }

    //     [Fact, Priority(50)]
    //     public async Task PersonRemove() {
    //         await using WebApplicationFactory<Program> application = new WebApplicationFactory<Program>();
    //         using (HttpClient client = application.CreateClient()) {
    //         }
    //     }
    // }
}
