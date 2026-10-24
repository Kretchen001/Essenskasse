namespace Modells.Test {

    public class ProviderListingTest {

        private static readonly Provider Bakery = new Provider(Guid.NewGuid(), "Baeckerei Meier");
        private static readonly Provider Pizza = new Provider(Guid.NewGuid(), "pizza Roma");
        private static readonly Provider Asia = new Provider(Guid.NewGuid(), "Asia Imbiss");
        private static readonly List<Provider> All = new() { Pizza, Bakery, Asia };

        private static List<string> Names(IEnumerable<Provider> providers) => providers.Select(x => x.Designation).ToList();

        [Fact]
        public void NameAsc_SortsIgnoringCase() {
            Assert.Equal(new[] { "Asia Imbiss", "Baeckerei Meier", "pizza Roma" }, Names(ProviderListing.Apply(All, "", ProviderSort.NameAsc)));
        }

        [Fact]
        public void NameDesc_SortsReverse() {
            Assert.Equal(new[] { "pizza Roma", "Baeckerei Meier", "Asia Imbiss" }, Names(ProviderListing.Apply(All, null, ProviderSort.NameDesc)));
        }

        [Theory]
        [InlineData("PIZZA", 1)]
        [InlineData("  imbiss ", 1)]
        [InlineData("r", 2)]
        [InlineData("gibtsnicht", 0)]
        [InlineData("", 3)]
        [InlineData("   ", 3)]
        public void Filter_IgnoresCaseAndSurroundingSpaces(string filter, int expected) {
            Assert.Equal(expected, ProviderListing.Apply(All, filter, ProviderSort.NameAsc).Count);
        }

        [Fact]
        public void LastOrderedFirst_PutsNewestFirstAndNeverOrderedLast() {
            Dictionary<Guid, ProviderStatistics> statistics = new() {
                { Pizza.Guid, new ProviderStatistics(Pizza.Guid, 1, 500, new DateTime(2026, 1, 10)) },
                { Bakery.Guid, new ProviderStatistics(Bakery.Guid, 4, 900, new DateTime(2026, 5, 2)) },
            };
            Assert.Equal(new[] { "Baeckerei Meier", "pizza Roma", "Asia Imbiss" },
                Names(ProviderListing.Apply(All, "", ProviderSort.LastOrderedFirst, statistics)));
        }

        [Fact]
        public void MostOrdersFirst_SortsByCountThenName() {
            Dictionary<Guid, ProviderStatistics> statistics = new() {
                { Pizza.Guid, new ProviderStatistics(Pizza.Guid, 3, 500, new DateTime(2026, 1, 10)) },
                { Bakery.Guid, new ProviderStatistics(Bakery.Guid, 3, 900, new DateTime(2026, 5, 2)) },
            };
            Assert.Equal(new[] { "Baeckerei Meier", "pizza Roma", "Asia Imbiss" },
                Names(ProviderListing.Apply(All, "", ProviderSort.MostOrdersFirst, statistics)));
        }

        [Fact]
        public void OrderSorts_WithoutStatistics_FallBackToName() {
            Assert.Equal(new[] { "Asia Imbiss", "Baeckerei Meier", "pizza Roma" },
                Names(ProviderListing.Apply(All, "", ProviderSort.LastOrderedFirst, null)));
        }

        [Fact]
        public void Apply_DoesNotChangeTheSourceList() {
            ProviderListing.Apply(All, "a", ProviderSort.NameAsc);
            Assert.Equal(new[] { "pizza Roma", "Baeckerei Meier", "Asia Imbiss" }, Names(All));
        }
    }
}
