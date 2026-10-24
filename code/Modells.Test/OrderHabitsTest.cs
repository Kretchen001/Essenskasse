using Modells.Statistics;

namespace Modells.Test {

    public class OrderHabitsTest {

        private static readonly Person Anna = new(Guid.NewGuid(), "Anna", null);
        private static readonly Provider Pizza = new("Pizza");
        private static readonly Provider Doener = new("Doener");

        // 2026-06-01 ist ein Montag.
        private static Order Ord(Provider p, int day, long euro = 5) => new(new DateTime(2026, 6, day, 12, 0, 0), new Money(euro, 0), p, Anna.Guid);

        private static StatisticsData Data(params Order[] orders) => new([Anna], [], orders);

        [Fact]
        public void WeekdayIndex_MondayIsZeroSundayIsSix() {
            Assert.Equal(0, OrderHabits.WeekdayIndex(new DateTime(2026, 6, 1)));
            Assert.Equal(6, OrderHabits.WeekdayIndex(new DateTime(2026, 6, 7)));
        }

        [Fact]
        public void Build_ProbabilityIsOrderDaysOverWeekdayOccurrences() {
            // Montage im Juni: 1., 8., 15., 22., 29. = 5. Bestellt an 2 davon.
            PersonHabit h = Assert.Single(OrderHabits.Build(Data(Ord(Pizza, 1), Ord(Pizza, 8)),
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 30), new DateTime(2026, 12, 1)));
            Assert.Equal(0.4, h.WeekdayProbability[0], 6);
            Assert.Equal(0, h.WeekdayProbability[1]);
            Assert.Equal(5, h.WeekdayDays[0]);
            Assert.Equal(2, h.OrderDays);
            Assert.True(h.SmallSample);
        }

        [Fact]
        public void Build_TwoOrdersOnTheSameDayCountOnce() {
            PersonHabit h = Assert.Single(OrderHabits.Build(Data(Ord(Pizza, 1), Ord(Doener, 1)),
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 7), new DateTime(2026, 12, 1)));
            Assert.Equal(1, h.OrderDays);
            Assert.Equal(2, h.Orders);
            Assert.Equal(1.0, h.WeekdayProbability[0], 6);
        }

        [Fact]
        public void Build_ReversedOrderDoesNotCount() {
            Order original = Ord(Pizza, 1);
            Order reversal = new(original.DateTime.AddTicks(1), new Money(-5, 0), Pizza, Anna.Guid);
            Assert.Empty(OrderHabits.Build(Data(original, reversal), new DateTime(2026, 6, 1), new DateTime(2026, 6, 30), new DateTime(2026, 12, 1)));
        }

        [Fact]
        public void Build_FavoriteProvidersSortedWithShares() {
            PersonHabit h = Assert.Single(OrderHabits.Build(Data(Ord(Pizza, 1), Ord(Pizza, 2), Ord(Pizza, 3), Ord(Doener, 4)),
                new DateTime(2026, 6, 1), new DateTime(2026, 6, 30), new DateTime(2026, 12, 1)));
            Assert.Equal("Pizza", h.TopProviders[0].Provider);
            Assert.Equal(0.75, h.TopProviders[0].Share, 6);
            Assert.Equal("Doener", h.TopProviders[1].Provider);
        }

        [Fact]
        public void Build_CapsObservationAtAsOf() {
            // Bis 8.6. beobachtet: Montage 1. und 8. => 2 Tage, 1 Bestelltag => 50 %.
            PersonHabit h = Assert.Single(OrderHabits.Build(Data(Ord(Pizza, 1)), new DateTime(2026, 6, 1), new DateTime(2026, 6, 30), new DateTime(2026, 6, 8)));
            Assert.Equal(2, h.WeekdayDays[0]);
            Assert.Equal(0.5, h.WeekdayProbability[0], 6);
        }
    }
}
