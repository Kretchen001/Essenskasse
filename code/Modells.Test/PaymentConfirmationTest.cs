using System.Globalization;

namespace Modells.Test {

    public class PaymentConfirmationTest {

        private const string Body = "Hallo {0}, am {1} um {2} kamen {3} an.";
        private const string CommentLine = "Anmerkung: {0}";
        private static readonly CultureInfo German = new CultureInfo("de-DE");
        private static readonly DateTime When = new DateTime(2026, 10, 22, 14, 5, 0);

        [Fact]
        public void Compose_FillsAllPlaceholdersAndAppendsComment() {
            (string subject, string body) = PaymentConfirmation.Compose("Betreff", Body, CommentLine, "Gruss",
                "Max", When, new Money(12, 50), "Fuer die Kasse", German);
            Assert.Equal("Betreff", subject);
            Assert.StartsWith("Hallo Max, am 22.10.2026 um 14:05 kamen ", body);
            Assert.Contains("12", body);
            Assert.Contains("\nAnmerkung: Fuer die Kasse", body);
            Assert.EndsWith("\n\nGruss", body);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Compose_WithoutComment_OmitsTheNoteLine(string? comment) {
            (_, string body) = PaymentConfirmation.Compose("Betreff", Body, CommentLine, "Gruss",
                "Max", When, new Money(5, 0), comment, German);
            Assert.DoesNotContain("Anmerkung", body);
            Assert.EndsWith("\n\nGruss", body);
        }

        [Fact]
        public void Compose_TrimsTheComment() {
            (_, string body) = PaymentConfirmation.Compose("B", Body, CommentLine, "G", "Max", When, new Money(1, 0), "  abc  ", German);
            Assert.Contains("Anmerkung: abc", body);
        }
    }
}
