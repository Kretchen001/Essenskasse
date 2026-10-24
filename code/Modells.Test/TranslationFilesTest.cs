using System.Text.Json;
using System.Text.RegularExpressions;

namespace Modells.Test {

    /// <summary>Stellt sicher, dass alle Sprachdateien (wwwroot/i18n) zu den Schluesseln im Code passen.
    /// Ein fehlender Eintrag wuerde in der App sonst nur als Schluesselname auftauchen.</summary>
    public class TranslationFilesTest {

        private static readonly string[] Cultures = ["de-DE", "en-US", "es-ES", "ru"];

        private static string ClientFolder() {
            DirectoryInfo? folder = new(AppContext.BaseDirectory);
            while (folder is not null) {
                string candidate = Path.Combine(folder.FullName, "Essenskasse", "wwwroot", "i18n");
                if (Directory.Exists(candidate)) {
                    return Path.Combine(folder.FullName, "Essenskasse");
                }
                folder = folder.Parent;
            }
            throw new DirectoryNotFoundException("Ordner Essenskasse/wwwroot/i18n nicht gefunden.");
        }

        private static Dictionary<string, string> Load(string culture) {
            string path = Path.Combine(ClientFolder(), "wwwroot", "i18n", culture + ".json");
            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;
        }

        private static string[] KeysInCode() {
            string source = File.ReadAllText(Path.Combine(ClientFolder(), "Ressources", "RessourceAppStrings.cs"));
            return Regex.Matches(source, @"^\s+(\w+),\s*$", RegexOptions.Multiline).Select(x => x.Groups[1].Value).ToArray();
        }

        [Theory]
        [InlineData("de-DE")]
        [InlineData("en-US")]
        [InlineData("es-ES")]
        [InlineData("ru")]
        public void EveryKeyInCodeHasATextAndNothingIsLeftOver(string culture) {
            HashSet<string> inFile = Load(culture).Keys.ToHashSet();
            string[] inCode = KeysInCode();
            Assert.Empty(inCode.Where(x => !inFile.Contains(x)));
            Assert.Empty(inFile.Where(x => !inCode.Contains(x)));
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("es-ES")]
        [InlineData("ru")]
        public void TextsAreNotEmptyAndKeepTheirPlaceholders(string culture) {
            Dictionary<string, string> german = Load("de-DE");
            Dictionary<string, string> other = Load(culture);
            foreach ((string key, string text) in german) {
                Assert.False(string.IsNullOrWhiteSpace(other[key]), $"{culture}: '{key}' ist leer");
                string[] placeholders = Regex.Matches(text, @"\{\d+\}").Select(x => x.Value).OrderBy(x => x).ToArray();
                string[] placeholdersOther = Regex.Matches(other[key], @"\{\d+\}").Select(x => x.Value).OrderBy(x => x).ToArray();
                Assert.True(placeholders.SequenceEqual(placeholdersOther), $"{culture}: Platzhalter von '{key}' weichen vom Deutschen ab");
            }
        }
    }
}
