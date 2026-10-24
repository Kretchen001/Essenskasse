using Modells.User;

namespace Modells.Test {

    public class CurrencyTest {

        [Theory]
        [InlineData("EUR", 7, 40, "7,40 €")]
        [InlineData("EUR", -8, -30, "-8,30 €")]
        [InlineData("EUR", 0, -5, "-0,05 €")]
        [InlineData("USD", 12, 5, "$12.05")]
        [InlineData("USD", -3, -50, "-$3.50")]
        [InlineData("GBP", 0, 99, "£0.99")]
        [InlineData("CHF", 10, 0, "10.00 CHF")]
        [InlineData("PLN", 1, 50, "1,50 zł")]
        [InlineData("SEK", 100, 0, "100,00 kr")]
        public void ToString_FormatsInTheGivenCurrency(string code, long euro, short cent, string expected) {
            Assert.Equal(expected, new Money(euro, cent).ToString(Currency.FromCode(code)));
        }

        [Fact]
        public void DefaultToString_IsEuro() {
            Assert.Equal(Currency.Euro, Money.CurrentCurrency);
            Assert.Equal("1,00 €", new Money(1, 0).ToString());
        }

        [Fact]
        public void FromCode_UnknownOrEmpty_IsEuro() {
            Assert.Equal(Currency.Euro, Currency.FromCode(null));
            Assert.Equal(Currency.Euro, Currency.FromCode(""));
            Assert.Equal(Currency.Euro, Currency.FromCode("XXX"));
        }

        [Fact]
        public void AllCurrencies_HaveUniqueCodes() {
            Assert.Equal(Currency.All.Count, Currency.All.Select(x => x.Code).Distinct().Count());
        }

        [Theory]
        [InlineData("$12.50", 1250)]
        [InlineData("12,50 zł", 1250)]
        [InlineData("-£0.05", -5)]
        [InlineData("10.00 CHF", 1000)]
        [InlineData("7,40 €", 740)]
        public void ToMoney_ReadsAnyCurrencyText(string text, long cents) {
            Assert.Equal(cents, new Money().ToMoney(text).ToTotalCents());
        }

        [Fact]
        public void ToMoney_RoundTripsInEveryCurrency() {
            foreach (Currency currency in Currency.All) {
                foreach (Money m in new Money[] { new(7, 40), new(0, 5), new(0, -5), new(-8, -30), new(1234, 0) }) {
                    Assert.Equal(m.ToTotalCents(), new Money().ToMoney(m.ToString(currency)).ToTotalCents());
                }
            }
        }

        [Theory]
        [InlineData("$12.50", 1250)]
        [InlineData("12,50 kr", 1250)]
        public void Parser_AcceptsOtherCurrencySigns(string text, long cents) {
            Assert.True(MoneyParser.TryParse(text, out Money money));
            Assert.Equal(cents, money.ToTotalCents());
        }

        [Fact]
        public void Sanitizer_KeepsKnownCurrencyAndDropsUnknown() {
            Assert.Equal("USD", SettingsSanitizer.Sanitize(new Settings { Currency = "USD" }).Currency);
            Assert.Equal("", SettingsSanitizer.Sanitize(new Settings { Currency = "BTC" }).Currency);
            Assert.Equal("", SettingsSanitizer.Sanitize(new Settings()).Currency);
        }
    }
}
