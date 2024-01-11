namespace Essenskasse.Client {
    public static class LocalizerSettings {

        private static CultureWithName neutralCulture = new CultureWithName(
            name: "Deutsch",
            culture: "de-DE"
        );

        public static CultureWithName NeutralCulture {
            get => neutralCulture;
            set => neutralCulture = value;
        }

        public static readonly List<CultureWithName> SupportedCultures = new List<CultureWithName>() {
            new CultureWithName("Deutsch", "de-DE"),
            new CultureWithName("English", "en-US"),
            new CultureWithName("Русский", "ru"),
            new CultureWithName("Español", "es-ES")
        };

    }
}
