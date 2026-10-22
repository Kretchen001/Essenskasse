namespace Modells.Test {

    public class QuickAmountsTest {

        [Fact]
        public void Normalize_SortsDistinctAndDropsInvalid() {
            Assert.Equal(new long[] { 50, 250, 1000 }, QuickAmounts.Normalize([1000, 250, 250, 0, -5, 50]));
        }

        [Fact]
        public void Normalize_NullOrEmpty_GivesDefault() {
            Assert.Equal(QuickAmounts.Default, QuickAmounts.Normalize(null));
            Assert.Equal(QuickAmounts.Default, QuickAmounts.Normalize([]));
            Assert.Equal(QuickAmounts.Default, QuickAmounts.Normalize([0, -1]));
        }

        [Fact]
        public void Normalize_LimitsCount() {
            Assert.Equal(QuickAmounts.MaxCount, QuickAmounts.Normalize(Enumerable.Range(1, 40).Select(x => (long)x * 10)).Count);
        }

        [Theory]
        [InlineData("3", 300)]
        [InlineData("2,5", 250)]
        [InlineData("0.75", 75)]
        [InlineData(" 12,00 € ", 1200)]
        public void TryAdd_ValidAmount_IsAddedInOrder(string text, long expected) {
            Assert.True(QuickAmounts.TryAdd([100, 500], text, out List<long> result));
            Assert.Contains(expected, result);
            Assert.Equal(result.OrderBy(x => x), result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("-2")]
        [InlineData("1,234")]
        [InlineData("999999999")]
        public void TryAdd_InvalidAmount_IsRejectedAndListStays(string text) {
            Assert.False(QuickAmounts.TryAdd([100, 500], text, out List<long> result));
            Assert.Equal(new long[] { 100, 500 }, result);
        }

        [Fact]
        public void TryAdd_Existing_IsNotDuplicated() {
            Assert.True(QuickAmounts.TryAdd([100, 500], "5", out List<long> result));
            Assert.Equal(new long[] { 100, 500 }, result);
        }

        [Fact]
        public void TryAdd_AtMaxCount_IsRejected() {
            List<long> full = Enumerable.Range(1, QuickAmounts.MaxCount).Select(x => (long)x * 100).ToList();
            Assert.False(QuickAmounts.TryAdd(full, "99", out List<long> result));
            Assert.Equal(full, result);
        }

        [Fact]
        public void Remove_KeepsAtLeastOne() {
            Assert.Equal(new long[] { 500 }, QuickAmounts.Remove([100, 500], 100));
            Assert.Equal(new long[] { 500 }, QuickAmounts.Remove([500], 500));
        }

        [Theory]
        [InlineData(500, "5 €")]
        [InlineData(100, "1 €")]
        [InlineData(250, "2,50 €")]
        [InlineData(5, "0,05 €")]
        public void Label_IsCompact(long cents, string expected) {
            Assert.Equal(expected, QuickAmounts.Label(cents));
        }
    }
}
