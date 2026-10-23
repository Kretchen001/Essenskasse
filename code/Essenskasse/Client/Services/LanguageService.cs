using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Globalization;

namespace Essenskasse.Client.Services {

    /// <summary>Wechselt die Sprache der App im laufenden Betrieb. Alle Texte sind bereits geladen (siehe TextService),
    /// deshalb genuegt es, die Kultur zu setzen und die Oberflaeche neu aufzubauen (MainLayout hoert auf <see cref="Changed"/>).</summary>
    public sealed class LanguageService {

        private const string CultureKey = "culture";

        private readonly ILocalStorageService _storage;
        private readonly IJSRuntime _js;

        public LanguageService(ILocalStorageService storage, IJSRuntime js) {
            _storage = storage;
            _js = js;
        }

        /// <summary>Wird nach jedem Sprachwechsel ausgeloest.</summary>
        public event Action? Changed;

        /// <summary>Zaehlt die Sprachwechsel; als <c>@key</c> baut sich damit die Oberflaeche neu auf.</summary>
        public int Version { get; private set; }

        /// <summary>Stellt die Sprache um und merkt sie sich. Unbekannte Sprachen und die bereits aktive werden ignoriert.</summary>
        public async Task SetAsync(string culture) {
            if (!LocalizerSettings.SupportedCultures.Any(x => x.Culture == culture)) {
                return;
            }
            await _storage.SetItemAsync(CultureKey, culture);
            if (CultureInfo.CurrentUICulture.Name == culture) {
                return;
            }

            CultureInfo info = new(culture);
            CultureInfo.DefaultThreadCurrentCulture = info;
            CultureInfo.DefaultThreadCurrentUICulture = info;
            CultureInfo.CurrentCulture = info;
            CultureInfo.CurrentUICulture = info;

            await SyncDocumentLanguageAsync();
            Version++;
            Changed?.Invoke();
        }

        /// <summary>Setzt das lang-Attribut der Seite auf die aktuelle Sprache (Screenreader, Silbentrennung).</summary>
        public async Task SyncDocumentLanguageAsync() {
            try {
                await _js.InvokeVoidAsync("SetDocumentLanguage", CultureInfo.CurrentUICulture.Name);
            }
            catch (JSException) {
                // rein kosmetisch
            }
        }
    }
}
