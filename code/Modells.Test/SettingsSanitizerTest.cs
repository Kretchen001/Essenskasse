using Modells.User;

namespace Modells.Test {

    public class SettingsSanitizerTest {

        private static Settings Valid() => new() {
            LoadAmountPayment = 10,
            LoadAmountOrder = 25,
            AdminEmail = "admin@example.org",
            Theme = "Dark",
            Language = "ru",
            PersonSort = "NameAsc",
            ProviderSort = "NameDesc",
            QuickAmounts = [500, 100]
        };

        [Fact]
        public void Sanitize_KeepsValidValues() {
            Settings result = SettingsSanitizer.Sanitize(Valid());
            Assert.Equal(10, result.LoadAmountPayment);
            Assert.Equal(25, result.LoadAmountOrder);
            Assert.Equal("admin@example.org", result.AdminEmail);
            Assert.Equal("Dark", result.Theme);
            Assert.Equal("ru", result.Language);
            Assert.Equal("NameAsc", result.PersonSort);
            Assert.Equal("NameDesc", result.ProviderSort);
            Assert.Equal(new long[] { 100, 500 }, result.QuickAmounts);
        }

        [Theory]
        [InlineData(-5, 1)]
        [InlineData(0, 1)]
        [InlineData(501, 500)]
        [InlineData(int.MaxValue, 500)]
        [InlineData(42, 42)]
        public void Sanitize_ClampsLoadAmounts(int input, int expected) {
            Settings settings = Valid();
            settings.LoadAmountPayment = input;
            settings.LoadAmountOrder = input;
            Settings result = SettingsSanitizer.Sanitize(settings);
            Assert.Equal(expected, result.LoadAmountPayment);
            Assert.Equal(expected, result.LoadAmountOrder);
        }

        [Fact]
        public void Sanitize_NeverKeepsAccessToken() {
            Settings settings = Valid();
            settings.AccessToken = Guid.NewGuid();
            Assert.Null(SettingsSanitizer.Sanitize(settings).AccessToken);
        }

        [Fact]
        public void Sanitize_DropsUnknownThemeLanguageAndSort() {
            Settings settings = Valid();
            settings.Theme = "Neon";
            settings.Language = "xx";
            settings.PersonSort = "Name'; DROP TABLE";
            settings.ProviderSort = new string('a', 33);
            Settings result = SettingsSanitizer.Sanitize(settings);
            Assert.Equal("", result.Theme);
            Assert.Equal("", result.Language);
            Assert.Equal("", result.PersonSort);
            Assert.Equal("", result.ProviderSort);
        }

        [Fact]
        public void Sanitize_ReplacesUndefinedMailMode() {
            Settings settings = Valid();
            settings.SendConfirmationMail = (ConfirmationMailMode)99;
            Assert.Equal(ConfirmationMailMode.Optional, SettingsSanitizer.Sanitize(settings).SendConfirmationMail);
        }

        [Fact]
        public void Sanitize_TrimsAndShortensEmail() {
            Settings settings = Valid();
            settings.AdminEmail = "  " + new string('a', 300) + "  ";
            Assert.Equal(SettingsSanitizer.MaxEmailLength, SettingsSanitizer.Sanitize(settings).AdminEmail.Length);
        }

        [Fact]
        public void Sanitize_EmptyQuickAmountsStayEmpty() {
            Settings settings = Valid();
            settings.QuickAmounts = [];
            Assert.Empty(SettingsSanitizer.Sanitize(settings).QuickAmounts);
        }

        [Fact]
        public void QuickAmountsText_RoundTrips() {
            string text = SettingsSanitizer.QuickAmountsToText([1000, 100, 250]);
            Assert.Equal("100,250,1000", text);
            Assert.Equal(new long[] { 100, 250, 1000 }, SettingsSanitizer.QuickAmountsFromText(text));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("abc,-5")]
        public void QuickAmountsFromText_EmptyOrUnreadable_GivesEmptyList(string? text) {
            Assert.Empty(SettingsSanitizer.QuickAmountsFromText(text));
        }

        [Fact]
        public void Settings_KeepsNewFieldsInNewtonsoftJson() {
            Settings back = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(Newtonsoft.Json.JsonConvert.SerializeObject(Valid()))!;
            Assert.Equal("Dark", back.Theme);
            Assert.Equal("ru", back.Language);
            Assert.Equal("NameAsc", back.PersonSort);
            Assert.Equal(new long[] { 500, 100 }, back.QuickAmounts);
        }

        [Fact]
        public void Settings_KeepsNewFieldsInSystemTextJson() {
            Settings back = System.Text.Json.JsonSerializer.Deserialize<Settings>(System.Text.Json.JsonSerializer.Serialize(Valid()))!;
            Assert.Equal("Dark", back.Theme);
            Assert.Equal("NameDesc", back.ProviderSort);
            Assert.Equal(new long[] { 500, 100 }, back.QuickAmounts);
        }
    }
}
