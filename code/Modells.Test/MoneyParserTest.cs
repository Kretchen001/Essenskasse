namespace Modells.Test {

    public class MoneyParserTest {

        [Theory]
        [InlineData("12", 1200)]
        [InlineData("12,5", 1250)]
        [InlineData("12.5", 1250)]
        [InlineData("12,50", 1250)]
        [InlineData("12.05", 1205)]
        [InlineData("0,99", 99)]
        [InlineData(",50", 50)]
        [InlineData("  7,00 € ", 700)]
        [InlineData("1.234,56", 123456)]
        [InlineData("1,234.56", 123456)]
        [InlineData("-3", -300)]
        [InlineData("-0,05", -5)]
        [InlineData("0", 0)]
        [InlineData("10.29", 1029)]
        public void ValidInput_IsConvertedExactlyToCents(string input, long expectedCents) {
            Assert.True(MoneyParser.TryParse(input, out Money money));
            Assert.Equal(expectedCents, money.ToTotalCents());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("12,345")]
        [InlineData("1.234")]
        [InlineData("12,")]
        [InlineData("1,2,3")]
        [InlineData("1.2.3")]
        [InlineData("--5")]
        [InlineData("5-")]
        [InlineData("-")]
        [InlineData(",")]
        [InlineData("1e3")]
        [InlineData("1234567890")]
        public void InvalidInput_IsRejected(string? input) {
            Assert.False(MoneyParser.TryParse(input, out Money money));
            Assert.Equal(0, money.ToTotalCents());
        }

        [Fact]
        public void NegativeAmount_KeepsSignOnEuroAndCent() {
            Assert.True(MoneyParser.TryParse("-12,34", out Money money));
            Assert.Equal(-12, money.Euro);
            Assert.Equal(-34, money.Cent);
        }
    }
}
