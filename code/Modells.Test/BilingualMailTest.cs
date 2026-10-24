namespace Modells.Test {

    public class BilingualMailTest {

        [Fact]
        public void Combine_NoParts_IsEmpty() {
            Assert.Equal(("", ""), BilingualMail.Combine([]));
        }

        [Fact]
        public void Combine_OneLanguage_StaysUnchanged() {
            Assert.Equal(("Betreff", "Text"), BilingualMail.Combine([new MailPart("Deutsch", "Betreff", "Text")]));
        }

        [Fact]
        public void Combine_TwoLanguages_KeepsOrderAndNamesBoth() {
            (string subject, string body) = BilingualMail.Combine([
                new MailPart("Deutsch", "Hallo", "Guten Tag"),
                new MailPart("English", "Hello", "Good day")]);
            Assert.Equal("Hallo / Hello", subject);
            Assert.True(body.IndexOf("Deutsch") < body.IndexOf("Guten Tag"));
            Assert.True(body.IndexOf("Guten Tag") < body.IndexOf("English"));
            Assert.True(body.IndexOf("English") < body.IndexOf("Good day"));
            Assert.Contains(BilingualMail.Divider, body);
        }

        [Fact]
        public void Combine_SameSubject_IsNotRepeated() {
            (string subject, _) = BilingualMail.Combine([new MailPart("A", "Kasse", "1"), new MailPart("B", "Kasse", "2")]);
            Assert.Equal("Kasse", subject);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("de-DE", "de-DE")]
        [InlineData("xx", "")]
        [InlineData(null, "")]
        public void SafeLanguage_KeepsOnlyKnownCodes(string? input, string expected) {
            Assert.Equal(expected, Modells.User.SettingsSanitizer.SafeLanguage(input));
        }
    }
}
