using Modells.Statistics;

namespace Modells.Test {

    public class MinusAnalysisTest {

        private static readonly DateTime Day0 = new(2026, 1, 1);

        private static Person Max(long euro) => new(Guid.NewGuid(), "Max", null, new Money(euro, 0));

        private static StatisticsData Data(Person p, params (int day, long euro)[] changes) {
            List<Payment> pay = [];
            List<Order> ord = [];
            foreach ((int day, long euro) in changes) {
                if (euro >= 0) {
                    pay.Add(new Payment(Day0.AddDays(day).AddHours(1), new Money(euro, 0), p.Guid, ""));
                }
                else {
                    ord.Add(new Order(Day0.AddDays(day).AddHours(1), new Money(-euro, 0), new Provider(), p.Guid));
                }
            }
            return new StatisticsData([p], pay, ord);
        }

        [Fact]
        public void Analyze_NeverBelowLimit_ZeroShare() {
            Person p = Max(10);
            PersonMinus r = Assert.Single(MinusAnalysis.Analyze(Data(p), Day0, Day0.AddDays(9), 0, Day0.AddDays(100)));
            Assert.Equal(TimeSpan.Zero, r.InMinus);
            Assert.Equal(0, r.Phases);
            Assert.False(r.InMinusAtEnd);
        }

        [Fact]
        public void Analyze_OnePhaseUntilPayment() {
            // Anfangssaldo 5, Tag 2: Bestellung -5 => 0 (Minus, weil nicht ueber dem Grenzwert), Tag 6: Einzahlung +10 => 10.
            Person p = Max(10);
            PersonMinus r = Assert.Single(MinusAnalysis.Analyze(Data(p, (2, -5), (6, 10)), Day0, Day0.AddDays(9), 0, Day0.AddDays(100)));
            Assert.Equal(TimeSpan.FromDays(4), r.InMinus);
            Assert.Equal(1, r.Phases);
            Assert.Equal(TimeSpan.FromDays(4), r.Longest);
            Assert.Equal(0.4, r.Share, 6);
            Assert.False(r.InMinusAtEnd);
        }

        [Fact]
        public void Analyze_LimitIsStrict_EqualCountsAsMinus() {
            Person p = Max(0);
            PersonMinus r = Assert.Single(MinusAnalysis.Analyze(Data(p), Day0, Day0.AddDays(1), 0, Day0.AddDays(100)));
            Assert.Equal(1.0, r.Share, 6);
            Assert.True(r.InMinusAtEnd);
        }

        [Fact]
        public void Analyze_TwoPhases_LongestIsTheBigger() {
            // Anfangssaldo 10: Tag 1 => -10 (Minus), Tag 3 => 10, Tag 5 => -10 (Minus), Tag 9 => 10.
            Person p = Max(10);
            StatisticsData d = Data(p, (1, -20), (3, 20), (5, -20), (9, 20));
            PersonMinus r = Assert.Single(MinusAnalysis.Analyze(d, Day0, Day0.AddDays(9), 0, Day0.AddDays(100)));
            Assert.Equal(2, r.Phases);
            Assert.Equal(TimeSpan.FromDays(6), r.InMinus);
            Assert.Equal(TimeSpan.FromDays(4), r.Longest);
        }

        [Fact]
        public void Analyze_CapsAtAsOf() {
            Person p = Max(-5);
            PersonMinus r = Assert.Single(MinusAnalysis.Analyze(Data(p), Day0, Day0.AddDays(30), 0, Day0.AddDays(5)));
            Assert.Equal(TimeSpan.FromDays(5), r.Observed);
            Assert.Equal(1.0, r.Share, 6);
        }

        [Fact]
        public void Analyze_PhaseRunningAtStartCountsOnce() {
            Person p = Max(-5);
            PersonMinus r = Assert.Single(MinusAnalysis.Analyze(Data(p), Day0, Day0.AddDays(3), 0, Day0.AddDays(100)));
            Assert.Equal(1, r.Phases);
            Assert.True(r.InMinusAtEnd);
        }

        [Fact]
        public void Analyze_EmptyRange_IsEmpty() {
            Assert.Empty(MinusAnalysis.Analyze(Data(Max(1)), Day0.AddDays(5), Day0, 0, Day0.AddDays(100)));
        }
    }
}
