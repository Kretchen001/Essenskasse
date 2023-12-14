namespace Essenskasse.Client {

    public record CultureWithName {

        public string Name { get; set; } = default!;
        public string Culture { get; set; } = default!;

        public CultureWithName(string name, string culture) {
            Name = name;
            Culture = culture;
        }
    }
}
