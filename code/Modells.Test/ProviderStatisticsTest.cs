using Newtonsoft.Json;

namespace Modells.Test {

    public class ProviderStatisticsTest {

        [Fact]
        public void Total_IsBuiltFromCents() {
            ProviderStatistics statistics = new ProviderStatistics(Guid.NewGuid(), 2, 1250, null);
            Assert.Equal(1250, statistics.Total.ToTotalCents());
        }

        [Fact]
        public void Json_RoundTrip_KeepsValuesAndOmitsTotal() {
            Guid guid = Guid.NewGuid();
            ProviderStatistics statistics = new ProviderStatistics(guid, 3, 4711, new DateTime(2026, 3, 4, 12, 0, 0));
            string json = JsonConvert.SerializeObject(statistics);
            Assert.DoesNotContain("\"Total\"", json);

            ProviderStatistics? back = JsonConvert.DeserializeObject<ProviderStatistics>(json);
            Assert.NotNull(back);
            Assert.Equal(guid, back!.ProviderGuid);
            Assert.Equal(3, back.OrderCount);
            Assert.Equal(4711, back.TotalCents);
            Assert.Equal(new DateTime(2026, 3, 4, 12, 0, 0), back.LastOrder);
        }

        [Fact]
        public void NeverOrdered_HasNoLastOrder() {
            ProviderStatistics statistics = new ProviderStatistics();
            Assert.Null(statistics.LastOrder);
            Assert.Equal(0, statistics.OrderCount);
        }
    }
}
