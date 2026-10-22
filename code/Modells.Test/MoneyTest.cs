namespace Modells.Test {

    public class MoneyTest {

        [Fact]
        public void ToStringTest_BothPositiv() {
            Assert.Equal("7,40 €", new Money(7, 40).ToString());
            Assert.Equal("3,80 €", new Money(3, 80).ToString());
        }

        [Fact]
        public void ToStringTest_EuroNegativ() {
            Assert.Equal("-8,30 €", new Money(-8, 30).ToString());
        }

        [Fact]
        public void ToStringTest_CentNegativ() {
            Assert.Equal("-0,05 €", new Money(0, -05).ToString());
            Assert.Equal("-0,50 €", new Money(0, -50).ToString());
        }

        [Fact]
        public void ToStringTest_CentPositivSmallerTen() {
            Assert.Equal("0,05 €", new Money(0, 5).ToString());
        }

        [Fact]
        public void ToStringTest_BothNegativ() {
            Assert.Equal("-8,30 €", new Money(-8, -30).ToString());
        }

        [Fact]
        public void IsPositivTest_Positiv() {
            Assert.True(new Money(2, 20).IsPositiv());
            Assert.True(new Money(0, 95).IsPositiv());
            Assert.False(new Money(-8, -30).IsPositiv());
        }

        [Fact]
        public void IsPositivTest_Negativ() {
            Assert.False(new Money(-22, 20).IsPositiv());
            Assert.False(new Money(0, -95).IsPositiv());
            Assert.True(new Money(8, 30).IsPositiv());
        }

        [Fact]
        public void EqualsTest() {
            Money a = new Money(1, 0);
            Assert.True(a.Equals(new Money(1, 0)));
            Assert.False(a.Equals(new Money(2, 92)));
        }

        [Fact]
        public void CalculateAddition() {
            Money a = new Money(20, 0);
            Assert.True(a.Equals(new Money().CalculateMoney(CalculationOperation.Addition,
                                                                new Money(10, 0),
                                                                new Money(10, 0))!));
            Assert.True(a.Equals(new Money().CalculateMoney(CalculationOperation.Addition,
                                                                new Money(9, 95),
                                                                new Money(10, 5))!));
            Assert.False(a.Equals(new Money().CalculateMoney(CalculationOperation.Addition,
                                                                new Money(9, 95),
                                                                new Money(10, 15))!));
        }

        [Fact]
        public void CalculateSubtraction() {
            Money a = new Money(20, 0);
            Assert.True(a.Equals(new Money().CalculateMoney(CalculationOperation.Substraction,
                                                                new Money(40, 0),
                                                                new Money(20, 0))!));
            Assert.True(a.Equals(new Money().CalculateMoney(CalculationOperation.Substraction,
                                                                new Money(20, 0),
                                                                new Money(0, 0))!));
            Assert.True(a.Equals(new Money().CalculateMoney(CalculationOperation.Substraction,
                                                                new Money(19, 0),
                                                                new Money(-1, 0))!));
            Assert.False(a.Equals(new Money().CalculateMoney(CalculationOperation.Substraction,
                                                                new Money(9, 95),
                                                                new Money(10, 15))!));
        }

        [Fact]
        public void CalculateSubtraction_NegativeResult() {
            Money result = new Money().CalculateMoney(CalculationOperation.Substraction,
                                                        new Money(9, 95),
                                                        new Money(10, 15))!;
            Assert.True(result.Equals(new Money(0, -20)));
            Assert.Equal("-0,20 €", result.ToString());
            Assert.False(result.IsPositiv());
        }

        [Fact]
        public void CalculateAddition_NegativeAndPositiv() {
            Money result = new Money().CalculateMoney(CalculationOperation.Addition,
                                                        new Money(-5, 0),
                                                        new Money(2, 50))!;
            Assert.True(result.Equals(new Money(-2, -50)));
            Assert.Equal("-2,50 €", result.ToString());
        }

        [Fact]
        public void CalculateAddition_CarryOverAndLargeValues() {
            Money result = new Money().CalculateMoney(CalculationOperation.Addition,
                                                        new Money(1_000_000_000, 99),
                                                        new Money(0, 1))!;
            Assert.True(result.Equals(new Money(1_000_000_001, 0)));
        }

        [Fact]
        public void CalculateAddition_ThenSubtraction_GivesStartValue() {
            // viele Buchungen hintereinander duerfen keinen Cent verlieren
            Money saldo = new Money(0, 0);
            for (int i = 0; i < 1000; i += 1) {
                saldo = new Money().CalculateMoney(CalculationOperation.Addition, saldo, new Money(3, 33))!;
            }
            Assert.Equal(333_000, saldo.ToTotalCents());
            for (int i = 0; i < 1000; i += 1) {
                saldo = new Money().CalculateMoney(CalculationOperation.Substraction, saldo, new Money(3, 33))!;
            }
            Assert.Equal(0, saldo.ToTotalCents());
        }

        [Fact]
        public void TotalCents_RoundTrip() {
            foreach (long cents in new long[] { 0, 1, -1, 99, -99, 100, -100, 12345, -12345, 999_999_999_99 }) {
                Assert.Equal(cents, Money.FromTotalCents(cents).ToTotalCents());
            }
            Assert.Equal(new Money(-2, -50).ToString(), Money.FromTotalCents(-250).ToString());
        }

        [Fact]
        public void ToMoney_RoundTripWithToString() {
            Assert.True(new Money(7, 40).Equals(new Money().ToMoney("7,40 €")));
            Assert.True(new Money(0, 5).Equals(new Money().ToMoney("0,05 €")));
            Assert.True(new Money(0, 0).Equals(new Money().ToMoney("0,00€")));
            // das Vorzeichen darf nicht verloren gehen und gilt fuer Euro und Cent
            Assert.True(new Money(0, -5).Equals(new Money().ToMoney("-0,05 €")));
            Assert.True(new Money(-8, -30).Equals(new Money().ToMoney("-8,30 €")));
            foreach (Money m in new Money[] { new Money(7, 40), new Money(0, 5), new Money(0, -5), new Money(-8, -30), new Money(-12, -3) }) {
                Assert.Equal(m.ToTotalCents(), new Money().ToMoney(m.ToString()).ToTotalCents());
            }
        }

        [Fact]
        public void CalculateDefaultBranch() {
            Money? a = new Money().CalculateMoney(null, new Money(40, 0), new Money(20, 0));
            Assert.True(a is null);
        }

        [Fact]
        public void IsGreaterEuro() {
            Assert.True(new Money(10, 10).IsGreater(new Money(5, 10)));
        }

        [Fact]
        public void IsLess() {
            Assert.False(new Money(5, 10).IsGreater(new Money(10, 10)));
        }

        [Fact]
        public void IsGreaterCent() {
            Assert.True(new Money(5, 50).IsGreater(new Money(5, 10)));
        }
    }
}