using System.Globalization;
using System.Net.Http.Json;

namespace Essenskasse.Client.Localization {

    /// <summary>Haelt die Texte der Sprachen (<c>wwwroot/i18n/{Kultur}.json</c>). Beim Start wird nur die Startsprache geladen
    /// (Standard Deutsch); die uebrigen laedt <see cref="LoadAllAsync"/> erst beim Oeffnen der Einstellungen, damit die Sprache
    /// danach ohne Neuladen der Seite gewechselt werden kann. Es gilt die Sprache der aktuellen Kultur
    /// (<see cref="CultureInfo.CurrentUICulture"/>); fehlt ein Text, wird Deutsch benutzt.</summary>
    public sealed class TextService {

        private readonly HttpClient _http;
        private readonly Dictionary<string, Dictionary<string, string>> _texts = new(StringComparer.OrdinalIgnoreCase);

        public TextService(HttpClient http) {
            _http = http;
        }

        /// <summary>Laedt beim Start nur die Startsprache; scheitert das, ersatzweise Deutsch.</summary>
        public async Task LoadStartAsync() {
            string current = CultureInfo.CurrentUICulture.Name;
            string start = LocalizerSettings.SupportedCultures.Any(x => x.Culture == current)
                ? current
                : LocalizerSettings.NeutralCulture.Culture;
            await LoadAsync(start);
            if (!IsLoaded(start)) {
                await LoadAsync(LocalizerSettings.NeutralCulture.Culture);
            }
        }

        /// <summary>Laedt die Textdateien aller unterstuetzten Sprachen (bereits geladene werden uebersprungen).</summary>
        public async Task LoadAllAsync() {
            await Task.WhenAll(LocalizerSettings.SupportedCultures.Select(x => LoadAsync(x.Culture)));
        }

        /// <summary>Sind die Texte dieser Sprache geladen?</summary>
        public bool IsLoaded(string culture) => _texts.ContainsKey(culture);

        /// <summary>Laedt die Texte einer Sprache, falls noch nicht geschehen.</summary>
        public async Task LoadAsync(string culture) {
            if (_texts.ContainsKey(culture)) {
                return;
            }
            try {
                Dictionary<string, string>? texts = await _http.GetFromJsonAsync<Dictionary<string, string>>($"i18n/{culture}.json");
                if (texts is not null) {
                    _texts[culture] = texts;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException or TaskCanceledException) {
                // Offline und nicht im Cache oder defekte Datei: diese Sprache faellt auf Deutsch bzw. den Schluessel zurueck.
                Console.Error.WriteLine($"Texte fuer '{culture}' konnten nicht geladen werden: {ex.Message}");
            }
        }

        /// <summary>Text zum Schluessel in genau dieser Sprache (sonst Deutsch, sonst der Schluessel). Die Sprache muss geladen sein
        /// (<see cref="LoadAsync"/>); fuer Texte, die nicht in der Sprache der Oberflaeche stehen, z. B. in Mails.</summary>
        public string Get(string culture, string key) {
            if (_texts.TryGetValue(culture, out Dictionary<string, string>? texts) && texts.TryGetValue(key, out string? found)) {
                return found;
            }
            if (_texts.TryGetValue(LocalizerSettings.NeutralCulture.Culture, out Dictionary<string, string>? neutral) && neutral.TryGetValue(key, out string? fallback)) {
                return fallback;
            }
            return key;
        }

        /// <summary>Text zum Schluessel in der aktuellen Sprache (sonst Deutsch).</summary>
        public bool TryGet(string key, out string value) {
            foreach (string culture in CandidateCultures()) {
                if (_texts.TryGetValue(culture, out Dictionary<string, string>? texts) && texts.TryGetValue(key, out string? found)) {
                    value = found;
                    return true;
                }
            }
            value = "";
            return false;
        }

        /// <summary>Alle Texte der aktuellen Sprache.</summary>
        public IEnumerable<KeyValuePair<string, string>> GetAll() {
            foreach (string culture in CandidateCultures()) {
                if (_texts.TryGetValue(culture, out Dictionary<string, string>? texts)) {
                    return texts;
                }
            }
            return [];
        }

        private static IEnumerable<string> CandidateCultures() {
            CultureInfo current = CultureInfo.CurrentUICulture;
            yield return current.Name;
            foreach (CultureWithName supported in LocalizerSettings.SupportedCultures) {
                if (new CultureInfo(supported.Culture).TwoLetterISOLanguageName == current.TwoLetterISOLanguageName) {
                    yield return supported.Culture;
                }
            }
            yield return LocalizerSettings.NeutralCulture.Culture;
        }
    }
}
