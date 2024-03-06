namespace Modells.Test {

    public class MoneyTest {

        [Fact]
        public void ToStringTest_BothPositiv() {
            Assert.Equal("7,40€", new Money(7, 40).ToString());
            Assert.Equal("3,80€", new Money(3, 80).ToString());
        }

        [Fact]
        public void ToStringTest_EuroNegativ() {
            Assert.Equal("-8,30€", new Money(-8, 30).ToString());
        }

        [Fact]
        public void ToStringTest_CentNegativ() {
            Assert.Equal("-0,05€", new Money(0, -05).ToString());
            Assert.Equal("-0,50€", new Money(0, -50).ToString());
        }

        [Fact]
        public void ToStringTest_CentPositivSmallerTen() {
            Assert.Equal("0,05€", new Money(0, 5).ToString());
        }

        [Fact]
        public void ToStringTest_BothNegativ() {
            Assert.Equal("-8,30€", new Money(-8, -30).ToString());
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