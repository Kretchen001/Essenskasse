using Modells.Statistics;

namespace Modells.Test {

    public class MonthlyFlowTest {

        private static readonly Guid Max = Guid.NewGuid();

        private static Payment Pay(int month, int day, long euro) => new(new DateTime(2026, month, day, 10, 0, 0), new Money(euro, 0), Max, "");

        private static Order Ord(int month, int day, long euro) => new(new DateTime(2026, month, day, 12, 0, 0), new Money(euro, 0), new Provider(), Max);

        [Fact]
        public void Build_SumsPerMonthAndFillsEmptyMonths() {
            List<MonthFlow> months = MonthlyFlow.Build(
                [Pay(1, 5, 10), Pay(3, 1, 5)], [Ord(1, 6, 4), Ord(1, 7, 3)],
                new DateTime(2026, 1, 1), new DateTime(2026, 3, 31));
            Assert.Equal(3, months.Count);
            Assert.Equal(new MonthFlow(new DateTime(2026, 1, 1), 1000, 700), months[0]);
            Assert.Equal(new MonthFlow(new DateTime(2026, 2, 1), 0, 0), months[1]);
            Assert.Equal(new MonthFlow(new DateTime(2026, 3, 1), 500, 0), months[2]);
            Assert.Equal(300, months[0].NetCents);
        }

        [Fact]
        public void Build_CountsOnlyDaysInsideTheRange() {
            List<MonthFlow> months = MonthlyFlow.Build([Pay(1, 5, 10), Pay(1, 20, 7)], [], new DateTime(2026, 1, 10), new DateTime(2026, 1, 31));
            Assert.Equal(700, Assert.Single(months).PaymentCents);
        }

        [Fact]
        public void Build_ReversalReducesTheMonth() {
            Order original = Ord(2, 3, 6);
            Order reversal = new(original.DateTime.AddTicks(1), new Money(-6, 0), new Provider(), Max);
            List<MonthFlow> months = MonthlyFlow.Build([], [original, reversal], new DateTime(2026, 2, 1), new DateTime(2026, 2, 28));
            Assert.Equal(0, Assert.Single(months).OrderCents);
        }

        [Fact]
        public void Build_EndBeforeStart_IsEmpty() {
            Assert.Empty(MonthlyFlow.Build([], [], new DateTime(2026, 3, 1), new DateTime(2026, 2, 1)));
        }
    }
}
