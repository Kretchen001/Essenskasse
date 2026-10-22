namespace Modells.Test {

    public class WeekDaysTest {

        [Theory]
        [InlineData(2026, 10, 19)] // Montag
        [InlineData(2026, 10, 22)] // Donnerstag
        [InlineData(2026, 10, 25)] // Sonntag
        public void OfWeek_GivesMondayToSunday(int year, int month, int day) {
            IReadOnlyList<DateTime> week = WeekDays.OfWeek(new DateTime(year, month, day, 14, 30, 0));
            Assert.Equal(7, week.Count);
            Assert.Equal(new DateTime(2026, 10, 19), week[0]);
            Assert.Equal(DayOfWeek.Monday, week[0].DayOfWeek);
            Assert.Equal(new DateTime(2026, 10, 25), week[6]);
            Assert.Equal(DayOfWeek.Sunday, week[6].DayOfWeek);
        }

        [Fact]
        public void OfWeek_CrossesMonthAndYearBoundary() {
            IReadOnlyList<DateTime> week = WeekDays.OfWeek(new DateTime(2027, 1, 1));
            Assert.Equal(new DateTime(2026, 12, 28), week[0]);
            Assert.Equal(new DateTime(2027, 1, 3), week[6]);
        }
    }
}
