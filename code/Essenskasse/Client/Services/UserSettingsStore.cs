using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Modells;
using Newtonsoft.Json;
using System.Text;
using U = Modells.User;

namespace Essenskasse.Client.Services {

    /// <summary>Ergebnis des Abrufs der Einstellungen vom Server.</summary>
    public enum ServerSettingsResult {
        /// <summary>Einstellungen gefunden und uebernommen.</summary>
        Applied,
        /// <summary>Auf dem Server sind noch keine Einstellungen gespeichert; es gelten die lokalen bzw. Standardwerte.</summary>
        NothingSaved,
        /// <summary>Server nicht erreichbar oder Anfrage abgelehnt; es gelten die lokalen bzw. Standardwerte.</summary>
        Failed
    }

    /// <summary>Ergebnis des Sendens der Einstellungen an den Server.</summary>
    public enum ServerCallResult { Success, NoConnection, Rejected }

    /// <summary>Haelt die Einstellungen im Browser getrennt pro Benutzer.
    /// Angemeldete Benutzer haben ihre eigenen Schluessel ("user/{name}/..."), so erbt niemand die Einstellungen
    /// (und nie den Aenderungstoken) eines anderen am selben Geraet. Ohne Anmeldung gelten die geraeteweiten Schluessel.
    /// Die Sprache ist geraeteweit ("culture"), weil sie schon vor der Anmeldung gebraucht wird.</summary>
    public class UserSettingsStore {

        private const string CultureKey = "culture";
        private const string MigratedFlagKey = "SettingsMigrated";

        /// <summary>Geraeteweite Einstellungen, die beim ersten Anmelden einmalig zum Benutzer kopiert werden.
        /// Der Aenderungstoken gehoert bewusst nicht dazu.</summary>
        private static readonly string[] MigratedKeys = [
            "IsSingleWishListDashboard", "NoticeableOnRightSide", "loadAmountOfPayments", "loadAmountOfOrders",
            "filterShowProvider", "filterShowPerson", "DashboardMoneyLimit", "AdminEmail", "ConfirmationMailMode",
            "SendConfirmationMail", "OrderQuickAmounts", "UserTheme", "PersonSort", "ProviderSort", "PersonIsASC", "ProviderIsASC", "Currency"
        ];

        private readonly ILocalStorageService _storage;
        private readonly AppDataService _appData;
        private readonly ThemeService _theme;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly LanguageService _language;
        private readonly CurrencyService _currency;

        public UserSettingsStore(ILocalStorageService storage, AppDataService appData, ThemeService theme,
                                    HttpClient http, IConfiguration config, LanguageService language, CurrencyService currency) {
            _language = language;
            _currency = currency;
            _storage = storage;
            _appData = appData;
            _theme = theme;
            _http = http;
            _config = config;
        }

        /// <summary>Name des angemeldeten Benutzers; leer ohne Anmeldung.</summary>
        public string UserName { get; private set; } = "";

        /// <summary>Ab jetzt gelten die Einstellungen dieses Benutzers (leer = niemand angemeldet).</summary>
        public void UseUser(string? name) {
            UserName = (name ?? "").Trim();
        }

        private string Key(string key) {
            return UserName.Length == 0 ? key : $"user/{UserName.ToLowerInvariant()}/{key}";
        }

        #region Zugriff
        /// <summary>Liest einen Wert; fehlt er oder ist er unlesbar, gilt <paramref name="fallback"/>.</summary>
        public async Task<T> GetAsync<T>(string key, T fallback) {
            try {
                string storageKey = Key(key);
                if (!await _storage.ContainKeyAsync(storageKey)) {
                    return fallback;
                }
                T? value = await _storage.GetItemAsync<T>(storageKey);
                return value is null ? fallback : value;
            }
            catch {
                return fallback;
            }
        }

        public async Task SetAsync<T>(string key, T value) {
            await _storage.SetItemAsync(Key(key), value);
        }

        /// <summary>Liest einen unveraendert abgelegten Text (ohne JSON-Anfuehrungszeichen); null, wenn es ihn nicht gibt.</summary>
        public async Task<string?> GetRawAsync(string key) {
            try {
                return await _storage.GetItemAsStringAsync(Key(key));
            }
            catch {
                return null;
            }
        }

        public async Task SetRawAsync(string key, string value) {
            await _storage.SetItemAsStringAsync(Key(key), value);
        }

        public async Task RemoveAsync(string key) {
            await _storage.RemoveItemAsync(Key(key));
        }

        private async Task<bool> ContainsAsync(string key) {
            try {
                return await _storage.ContainKeyAsync(Key(key));
            }
            catch {
                return false;
            }
        }
        #endregion

        #region Laden lokal
        /// <summary>Uebernimmt einmalig die geraeteweiten Einstellungen (bisheriger Stand) zum ersten angemeldeten Benutzer.
        /// Ein frueher gespeicherter Aenderungstoken wird entfernt; er kann in den Einstellungen neu abgerufen werden.</summary>
        public async Task MigrateLegacyAsync() {
            if (UserName.Length == 0) {
                return;
            }
            try {
                if (await _storage.ContainKeyAsync(MigratedFlagKey)) {
                    return;
                }
                foreach (string key in MigratedKeys) {
                    if (await _storage.ContainKeyAsync(key) && !await ContainsAsync(key)) {
                        string? raw = await _storage.GetItemAsStringAsync(key);
                        if (raw is not null) {
                            await _storage.SetItemAsStringAsync(Key(key), raw);
                        }
                    }
                }
                await _storage.RemoveItemAsync("AccessToken");
                await _storage.SetItemAsync(MigratedFlagKey, true);
            }
            catch {
                // Ohne Migration starten die Einstellungen mit den Standardwerten.
            }
        }

        /// <summary>Setzt alle Einstellungen im Programm aus dem Browserspeicher des aktuellen Benutzers.
        /// Was fehlt, bekommt den Standardwert.</summary>
        public async Task LoadAppDataAsync() {
            AppDataService defaults = new();
            _appData.IsSingleListWishDashboard = await GetAsync("IsSingleWishListDashboard", defaults.IsSingleListWishDashboard);
            _appData.NoticeableOnRightSide = await GetAsync("NoticeableOnRightSide", defaults.NoticeableOnRightSide);
            _appData.LoadAmountPayment = InRange(await GetAsync("loadAmountOfPayments", defaults.LoadAmountPayment), defaults.LoadAmountPayment);
            _appData.LoadAmountOrder = InRange(await GetAsync("loadAmountOfOrders", defaults.LoadAmountOrder), defaults.LoadAmountOrder);
            _appData.FilterShowProvider = await GetAsync("filterShowProvider", defaults.FilterShowProvider);
            _appData.FilterShowPerson = await GetAsync("filterShowPerson", defaults.FilterShowPerson);
            _appData.DashboardMoneyLimit = await GetAsync("DashboardMoneyLimit", defaults.DashboardMoneyLimit);
            _appData.AdminEmail = await GetAsync("AdminEmail", "");
            _appData.SendConfirmationMail = await ReadConfirmationMailModeAsync();
            _appData.QuickAmounts = Modells.QuickAmounts.Normalize(await GetAsync<List<long>>("OrderQuickAmounts", []));
            _currency.Set(Modells.Currency.FromCode(await GetAsync("Currency", "")));
            // Der Aenderungstoken gibt es nur pro angemeldetem Benutzer.
            Guid token = UserName.Length == 0 ? Guid.Empty : await GetAsync("AccessToken", Guid.Empty);
            _appData.AccessToken = token == Guid.Empty ? null : token;
        }

        /// <summary>Waehlt die Waehrung der Anzeige, merkt sie sich beim Benutzer und stellt die Oberflaeche um.</summary>
        public async Task SetCurrencyAsync(string code) {
            Modells.Currency currency = Modells.Currency.FromCode(code);
            await SetAsync("Currency", currency.Code);
            _currency.Set(currency);
        }

        private static int InRange(int value, int fallback) {
            return value is >= U.SettingsSanitizer.MinLoadAmount and <= U.SettingsSanitizer.MaxLoadAmount ? value : fallback;
        }

        private async Task<U.ConfirmationMailMode> ReadConfirmationMailModeAsync() {
            if (await ContainsAsync("ConfirmationMailMode")) {
                return U.ConfirmationMailModes.Parse((await GetAsync("ConfirmationMailMode", 1)).ToString());
            }
            if (await ContainsAsync("SendConfirmationMail")) {
                return await GetAsync("SendConfirmationMail", true)
                    ? U.ConfirmationMailMode.Optional
                    : U.ConfirmationMailMode.Never;
            }
            return U.ConfirmationMailMode.Optional;
        }

        /// <summary>Das gemerkte Farbschema des aktuellen Benutzers; ohne Eintrag hell.</summary>
        public async Task<ColorCode> ReadThemeAsync() {
            return await GetRawAsync("UserTheme") == "Dark" ? ColorCode.Dark : ColorCode.Light;
        }

        /// <summary>Schreibt alle Werte aus dem Programm in den Browserspeicher des aktuellen Benutzers.</summary>
        public async Task PersistAppDataAsync() {
            await SetAsync("IsSingleWishListDashboard", _appData.IsSingleListWishDashboard);
            await SetAsync("NoticeableOnRightSide", _appData.NoticeableOnRightSide);
            await SetAsync("loadAmountOfPayments", _appData.LoadAmountPayment);
            await SetAsync("loadAmountOfOrders", _appData.LoadAmountOrder);
            await SetAsync("filterShowProvider", _appData.FilterShowProvider);
            await SetAsync("filterShowPerson", _appData.FilterShowPerson);
            await SetAsync("DashboardMoneyLimit", _appData.DashboardMoneyLimit);
            await SetAsync("AdminEmail", _appData.AdminEmail);
            await SetAsync("ConfirmationMailMode", (int)_appData.SendConfirmationMail);
            await RemoveAsync("SendConfirmationMail");
            await SetAsync("OrderQuickAmounts", _appData.QuickAmounts);
        }
        #endregion

        #region Server
        /// <summary>Die Einstellungen fuer den Upload: Programmwerte plus Farbschema, Sprache und Sortierung.
        /// Der Aenderungstoken gehoert nicht dazu.</summary>
        public async Task<U.Settings> BuildSettingsAsync() {
            U.Settings settings = _appData.GetAppDataAsSettingsObject();
            settings.Theme = _theme.ThemeCode.ToString();
            settings.Language = await CurrentLanguageAsync();
            settings.PersonSort = await GetAsync("PersonSort", "");
            settings.ProviderSort = await GetAsync("ProviderSort", "");
            settings.Currency = Modells.Money.CurrentCurrency.Code;
            return settings;
        }

        private async Task<string> CurrentLanguageAsync() {
            try {
                string? language = await _storage.GetItemAsync<string>(CultureKey);
                return string.IsNullOrWhiteSpace(language) ? LocalizerSettings.NeutralCulture.Culture : language;
            }
            catch {
                return LocalizerSettings.NeutralCulture.Culture;
            }
        }

        /// <summary>Uebernimmt Einstellungen vom Server ins Programm und in den Browserspeicher des Benutzers.
        /// Leere Angaben (nicht gespeichert) lassen die lokalen Werte unveraendert.
        /// Gibt true zurueck, wenn sich die Sprache geaendert hat und die Seite neu geladen werden muss.</summary>
        public async Task<bool> ApplyServerSettingsAsync(U.Settings settings) {
            _appData.SettingsToAppData(settings);
            if (settings.QuickAmounts.Count > 0) {
                _appData.QuickAmounts = Modells.QuickAmounts.Normalize(settings.QuickAmounts);
            }
            await PersistAppDataAsync();

            if (settings.Theme == "Dark" || settings.Theme == "Light") {
                _theme.ThemeCode = settings.Theme == "Dark" ? ColorCode.Dark : ColorCode.Light;
                await SetRawAsync("UserTheme", settings.Theme);
            }
            if (settings.PersonSort.Length > 0) {
                await SetAsync("PersonSort", settings.PersonSort);
            }
            if (settings.ProviderSort.Length > 0) {
                await SetAsync("ProviderSort", settings.ProviderSort);
            }
            if (Modells.Currency.IsKnown(settings.Currency)) {
                await SetCurrencyAsync(settings.Currency);
            }
            if (settings.Language.Length > 0 && settings.Language != await CurrentLanguageAsync()) {
                await _language.SetAsync(settings.Language); // wechselt sofort ohne Neuladen
                return true;
            }
            return false;
        }

        /// <summary>Fragt die gespeicherten Einstellungen des angemeldeten Benutzers beim Server ab und uebernimmt sie.
        /// Kommt nichts (noch nie gespeichert oder Fehler), bleibt alles unveraendert.</summary>
        public async Task<ServerSettingsResult> LoadFromServerAsync() {
            try {
                HttpRequestMessage request = await CreateRequestAsync(HttpMethod.Get, "user/loadSettingsForUser");
                HttpResponseMessage response = await _http.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent) {
                    return ServerSettingsResult.NothingSaved;
                }
                if (!response.IsSuccessStatusCode) {
                    return ServerSettingsResult.Failed;
                }
                U.Settings? settings = JsonConvert.DeserializeObject<U.Settings>(await response.Content.ReadAsStringAsync());
                if (settings is null) {
                    return ServerSettingsResult.NothingSaved;
                }
                await ApplyServerSettingsAsync(settings);
                return ServerSettingsResult.Applied;
            }
            catch {
                return ServerSettingsResult.Failed;
            }
        }

        /// <summary>Speichert die aktuellen Einstellungen des angemeldeten Benutzers auf dem Server.</summary>
        public async Task<ServerCallResult> SaveToServerAsync() {
            try {
                HttpRequestMessage request = await CreateRequestAsync(HttpMethod.Post, "user/saveSettingsForUser");
                request.Content = new StringContent(JsonConvert.SerializeObject(await BuildSettingsAsync()), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode ? ServerCallResult.Success : ServerCallResult.Rejected;
            }
            catch {
                return ServerCallResult.NoConnection;
            }
        }

        private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string path) {
            string name = await _storage.GetItemAsync<string>("NameOfUser") ?? "";
            Guid sessionToken = await _storage.GetItemAsync<Guid>("SessionToken");
            HttpRequestMessage request = new() {
                Method = method,
                RequestUri = new Uri(_config["API-Adress"] + path)
            };
            request.Headers.Add("sessionToken", sessionToken.ToString());
            request.Headers.Add("name", name);
            return request;
        }
        #endregion
    }
}
