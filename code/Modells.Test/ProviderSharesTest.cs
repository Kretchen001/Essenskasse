using Modells.Statistics;

namespace Modells.Test {

    public class ProviderSharesTest {

        private static readonly Guid Person = Guid.NewGuid();
        private static readonly Provider Pizza = new("Pizza");
        private static readonly Provider Doener = new("Doener");

        private static Order Ord(Provider p, int day, long euro) => new(new DateTime(2026, 5, day, 12, 0, 0), new Money(euro, 0), p, Person);

        [Fact]
        public void Build_GroupsSortsAndComputesShares() {
            List<ProviderShare> r = ProviderShares.Build([Ord(Pizza, 1, 6), Ord(Doener, 2, 5), Ord(Pizza, 3, 6), Ord(Doener, 4, 2)],
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 31));
            Assert.Equal(2, r.Count);
            Assert.Equal("Pizza", r[0].Name);
            Assert.Equal(1200, r[0].Cents);
            Assert.Equal(2, r[0].Count);
            Assert.Equal(600, r[0].AverageCents);
            Assert.Equal(1200.0 / 1900, r[0].AmountShare, 6);
            Assert.Equal(1.0, r.Sum(x => x.AmountShare), 6);
        }

        [Fact]
        public void Build_ReversalNetsOutAmountAndCount() {
            Order original = Ord(Pizza, 1, 6);
            Order reversal = new(original.DateTime.AddTicks(1), new Money(-6, 0), Pizza, Person);
            List<ProviderShare> r = ProviderShares.Build([original, reversal, Ord(Doener, 2, 5)], new DateTime(2026, 5, 1), new DateTime(2026, 5, 31));
            ProviderShare only = Assert.Single(r);
            Assert.Equal("Doener", only.Name);
        }

        [Fact]
        public void Build_RespectsRange() {
            Assert.Empty(ProviderShares.Build([Ord(Pizza, 1, 6)], new DateTime(2026, 6, 1), new DateTime(2026, 6, 30)));
        }
    }
}
