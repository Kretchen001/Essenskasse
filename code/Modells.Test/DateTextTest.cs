using System.Globalization;

namespace Modells.Test {

    public class DateTextTest {

        private static readonly DateTime Day = new(2026, 10, 23, 17, 24, 0);

        [Theory]
        [InlineData("de-DE", "23", "10")]
        [InlineData("en-US", "10", "23")]
        [InlineData("es-ES", "23", "10")]
        [InlineData("ru", "23", "10")]
        public void DayMonth_FollowsCultureOrderAndHasNoYear(string culture, string first, string second) {
            string text = DateText.DayMonth(Day, new CultureInfo(culture));
            Assert.DoesNotContain("2026", text);
            Assert.DoesNotContain("26", text.Replace(first, "").Replace(second, ""));
            Assert.True(text.IndexOf(first, StringComparison.Ordinal) < text.LastIndexOf(second, StringComparison.Ordinal), text);
        }

        [Fact]
        public void DayMonth_EnglishIsMonthFirst() {
            Assert.Equal("10/23", DateText.DayMonth(Day, new CultureInfo("en-US")));
        }

        [Fact]
        public void WeekdayDay_ContainsWeekdayAndDayNumber() {
            string text = DateText.WeekdayDay(Day, new CultureInfo("en-US"));
            Assert.StartsWith("Fri", text);
            Assert.EndsWith("23", text);
        }

        [Fact]
        public void WeekdayDate_ContainsYear() {
            Assert.Contains("2026", DateText.WeekdayDate(Day, new CultureInfo("de-DE")));
        }

        [Fact]
        public void WeekdayDayMonthTime_ContainsTime() {
            Assert.Contains("17", DateText.WeekdayDayMonthTime(Day, new CultureInfo("de-DE")));
        }
    }
}
