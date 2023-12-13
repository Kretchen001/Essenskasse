namespace Essenskasse.Client {
    public static class LocalizerSettings {

        public static CultureWithName NeutralCulture = new CultureWithName(
            name: "German",
            culture: "de-DE"
        );

        public static readonly List<CultureWithName> SupportedCultures = new List<CultureWithName>() {
            new CultureWithName("Deutsch", "de-DE"),
            new CultureWithName("English", "en-US"),
            new CultureWithName("Русский", "ru")
        };
    }
}
