using Modells.Statistics;

namespace Modells.Test {

    public class StatisticsTest {

        private static readonly Guid Max = Guid.NewGuid();

        private static BalanceEvent E(int month, int day, long cents) => new(new DateTime(2026, month, day, 12, 0, 0), Max, cents);

        [Fact]
        public void Data_SignsPaymentsPositiveAndOrdersNegative() {
            Guid person = Guid.NewGuid();
            StatisticsData data = new([], [new Payment(new DateTime(2026, 1, 2), new Money(10, 0), person, "")],
                [new Order(new DateTime(2026, 1, 1), new Money(4, 50), new Provider(), person)]);
            Assert.Equal([-450L, 1000L], data.Events.Select(x => x.Cents));
            Assert.Equal((new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)), data.Span);
        }

        [Fact]
        public void Data_ReversalAndOriginalCancelOut() {
            Guid person = Guid.NewGuid();
            Order original = new(new DateTime(2026, 1, 1, 12, 0, 0), new Money(4, 50), new Provider(), person);
            Order reversal = new(original.DateTime.AddTicks(1), new Money(-4, -50), new Provider(), person);
            StatisticsData data = new([], [], [original, reversal]);
            Assert.Equal(0, data.Events.Sum(x => x.Cents));
        }

        [Fact]
        public void Data_WithoutBookings_HasNoSpan() {
            Assert.Null(new StatisticsData([], [], []).Span);
        }

        [Fact]
        public void Daily_StartsWithTheBalanceBeforeTheRangeAndCarriesOverQuietDays() {
            BalanceEvent[] events = [E(1, 1, 1000), E(1, 3, -300), E(1, 5, 200)];
            List<DayValue> days = CashHistory.Daily(events, new DateTime(2026, 1, 2), new DateTime(2026, 1, 5));
            Assert.Equal([1000L, 700L, 700L, 900L], days.Select(x => x.Cents));
            Assert.Equal(new DateTime(2026, 1, 2), days[0].Day);
        }

        [Fact]
        public void Daily_SumsSeveralBookingsOfOneDay() {
            List<DayValue> days = CashHistory.Daily([E(1, 1, 100), E(1, 1, 250)], new DateTime(2026, 1, 1), new DateTime(2026, 1, 1));
            Assert.Equal(350, Assert.Single(days).Cents);
        }

        [Fact]
        public void Daily_EndBeforeStart_IsEmpty() {
            Assert.Empty(CashHistory.Daily([], new DateTime(2026, 2, 1), new DateTime(2026, 1, 1)));
        }

        [Fact]
        public void Downsample_KeepsFirstAndLastAndStaysSmall() {
            List<DayValue> points = Enumerable.Range(0, 1000).Select(i => new DayValue(new DateTime(2026, 1, 1).AddDays(i), i)).ToList();
            List<DayValue> small = CashHistory.Downsample(points, 200);
            Assert.True(small.Count <= 201);
            Assert.Equal(points[0], small[0]);
            Assert.Equal(points[^1], small[^1]);
            Assert.Equal(points, CashHistory.Downsample(points, 5000));
        }

        [Theory]
        [InlineData(0, 1000)]
        [InlineData(-4550, 12300)]
        [InlineData(500, 500)]
        [InlineData(-900, -100)]
        public void NiceTicks_CoverTheRangeInOrder(long min, long max) {
            List<long> ticks = ChartScale.NiceTicks(min, max);
            Assert.True(ticks.Count >= 2);
            Assert.True(ticks[0] <= min);
            Assert.True(ticks[^1] >= max);
            Assert.Equal(ticks.OrderBy(x => x), ticks);
            Assert.True(ticks.Count <= 8);
        }
    }
}
