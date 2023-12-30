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
    }
}