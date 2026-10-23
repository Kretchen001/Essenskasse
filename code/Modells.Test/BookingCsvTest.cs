namespace Modells.Test {

    public class BookingCsvTest {

        private static readonly string[] Headers = ["Datum", "Uhrzeit", "Art", "Person", "Anmerkung", "Betrag", "Waehrung"];
        private static readonly DateTime When = new(2026, 10, 23, 14, 5, 9);

        [Fact]
        public void Build_WritesHeaderAndRows() {
            string csv = BookingCsv.Build([
                new BookingRow(When, "Einzahlung", "Max", "Danke", new Money(12, 50)),
                new BookingRow(When, "Bestellung", "Eva", "Pizza", new Money(-7, -5))
            ], Headers, Currency.Euro);
            Assert.Equal(
                "Datum;Uhrzeit;Art;Person;Anmerkung;Betrag;Waehrung\r\n" +
                "2026-10-23;14:05:09;Einzahlung;Max;Danke;12,50;EUR\r\n" +
                "2026-10-23;14:05:09;Bestellung;Eva;Pizza;-7,05;EUR\r\n", csv);
        }

        [Fact]
        public void Build_UsesTheDecimalSeparatorOfTheCurrency() {
            string csv = BookingCsv.Build([new BookingRow(When, "k", "p", "d", new Money(3, 0))], Headers, Currency.FromCode("USD"));
            Assert.Contains(";3.00;USD", csv);
        }

        [Theory]
        [InlineData("Hallo", "Hallo")]
        [InlineData("a;b", "\"a;b\"")]
        [InlineData("sagt \"hi\"", "\"sagt \"\"hi\"\"\"")]
        [InlineData("zwei\nZeilen", "\"zwei\nZeilen\"")]
        [InlineData("=SUMME(A1)", "'=SUMME(A1)")]
        [InlineData("@Befehl", "'@Befehl")]
        [InlineData("-5 Euro", "'-5 Euro")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void Field_EscapesTextSafely(string? input, string expected) {
            Assert.Equal(expected, BookingCsv.Field(input));
        }

        [Theory]
        [InlineData(0, 5, "0,05")]
        [InlineData(-8, -30, "-8,30")]
        [InlineData(1234, 0, "1234,00")]
        public void Amount_FormatsWithTwoDecimals(long euro, short cent, string expected) {
            Assert.Equal(expected, BookingCsv.Amount(new Money(euro, cent), Currency.Euro));
        }
    }
}
