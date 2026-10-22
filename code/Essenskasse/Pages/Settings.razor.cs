using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Modells;
using U = Modells.User;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Json;

namespace Essenskasse.Pages {

    /// <summary></summary>
    public partial class Settings {

        #region Variable
        /// <summary></summary>
        ColorCode colorCode = 0;

        /// <summary></summary>
        private bool IsSingleListWish {
            get {
                return AppData.IsSingleListWishDashboard;
            }
            set {
                AppData.IsSingleListWishDashboard = value;
                WriteWishListToLocalStorage(value);
            }
        }

        /// <summary></summary>
        private string ReturnListWish {
            get {
                if (AppData.IsSingleListWishDashboard) {
                    return _loc[nameof(RessourceAppStrings.yes)];
                }
                else {
                    return _loc[nameof(RessourceAppStrings.no)];
                }
            }
        }

        /// <summary> </summary>
        private bool NoticeableOnRightSide {
            get {
                return AppData.NoticeableOnRightSide;
            }
            set {
                AppData.NoticeableOnRightSide = value;
                WriteNoticableSideToLocalStorage(value);
            }
        }

        /// <summary></summary>
        private string ReturnNoticeableSide{
            get {
                if (AppData.NoticeableOnRightSide) {
                    return _loc[nameof(RessourceAppStrings.yes)];
                }
                else {
                    return _loc[nameof(RessourceAppStrings.no)];
                }
            }
        }

        /// <summary>Gibt an, ob und wie die Bestaetigungsmail bei Einzahlungen vorbereitet wird.</summary>
        private U.ConfirmationMailMode SendConfirmationMail {
            get {
                return AppData.SendConfirmationMail;
            }
            set {
                AppData.SendConfirmationMail = value;
                WriteSendConfirmationMailToLocalStorage(value);
            }
        }

        /// <summary>Beschriftung eines Modus in der gewaehlten Sprache.</summary>
        private string MailModeText(U.ConfirmationMailMode mode) {
            return mode switch {
                U.ConfirmationMailMode.Never => _loc[nameof(RessourceAppStrings.confirmationMailNever)],
                U.ConfirmationMailMode.Always => _loc[nameof(RessourceAppStrings.confirmationMailAlways)],
                _ => _loc[nameof(RessourceAppStrings.confirmationMailOptional)]
            };
        }

        /// <summary></summary>
        private void ChangeTheme(string value) {
            if (value.Equals("Dark")) {
                colorCode = ColorCode.Dark;
            }
            else if (value.Equals("Light")) {
                colorCode = ColorCode.Light;
            }
            themeService.ThemeCode = colorCode;
        }

        /// <summary> Set the current selection to the ThemeService. </summary>
        private string CurrentTheme {
            set => ChangeTheme(value);
            get {
                return themeService.ThemeCode.ToString();
            }
        }

        /// <summary> Hold the entities for theme-selection. </summary>
        private readonly Dictionary<string, string> themeList = new Dictionary<string, string>() {
            { "Light", "Hell" },
            { "Dark", "Dunkel" }
        };

        /// <summary></summary>
        private int LoadAmountPayments {
            get => AppData.LoadAmountPayment;
            set {
                if (value < 1 || value > 500) {
                    return; // sinnvolle Grenzen, 0 oder negative Werte wuerden die Listen leeren
                }
                AppData.LoadAmountPayment = value;
                SaveAmountOfPaymentsPersistentAsync(value);
            }
        }

        /// <summary></summary>
        private int LoadAmountOrders {
            get => AppData.LoadAmountOrder;
            set {
                if (value < 1 || value > 500) {
                    return;
                }
                AppData.LoadAmountOrder = value;
                SaveAmountOfOrderPersistentAsync(value);
            }
        }

        /// <summary></summary>
        private bool FilterShowProvider {
            get => AppData.FilterShowProvider;
            set {
                AppData.FilterShowProvider = value;
                SaveFilterShowProvider(value);
            }
        }

        /// <summary></summary>
        private bool FilterShowPerson {
            get => AppData.FilterShowPerson;
            set {
                AppData.FilterShowPerson = value;
                SaveFilterShowPerson(value);
            }
        }

        /// <summary></summary>
        private string FilterShowProviderString {
            get {
                if (AppData.FilterShowProvider) {
                    return _loc[nameof(RessourceAppStrings.yes)];
                }
                else {
                    return _loc[nameof(RessourceAppStrings.no)];
                }
            }
        }

        /// <summary></summary>
        private string FilterShowPersonString {
            get {
                if (AppData.FilterShowPerson) {
                    return _loc[nameof(RessourceAppStrings.yes)];
                }
                else {
                    return _loc[nameof(RessourceAppStrings.no)];
                }
            }
        }

        /// <summary>Grenzwert als Text fuer das Eingabefeld, z. B. "10,50" (ohne Waehrungszeichen).</summary>
        private string LimitDisplay {
            get {
                long cents = AppData.DashboardMoneyLimit.ToTotalCents();
                string sign = cents < 0 ? "-" : "";
                cents = Math.Abs(cents);
                return sign + (cents / 100) + "," + (cents % 100).ToString("00");
            }
        }

        /// <summary>Der Grenzwert wird beim Verlassen des Feldes uebernommen; ungueltige Eingaben werden gemeldet und verworfen.</summary>
        private void OnLimitChanged(ChangeEventArgs e) {
            string? text = e.Value?.ToString();
            if (MoneyParser.TryParse(text, out Money parsed)) {
                DashboardMoneyLimit = parsed;
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.amountInvalid)], ToastLevel.Warning);
            }
        }
        /// <summary></summary>
        private Money DashboardMoneyLimit {
            get => AppData.DashboardMoneyLimit;
            set {
                AppData.DashboardMoneyLimit = value;
                SaveDashboardMoneyLimit(value);
            }
        }

        /// <summary></summary>
        private bool ExistAccessToken {
            get {
                if (AppData.AccessToken is not null) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary></summary>
        private string AdminEmail {
            get => AppData.AdminEmail;
            set {
                AppData.AdminEmail = value;
                SaveAdminEmail(value);
            }
        }
        #endregion

        #region Functions
        protected override void OnInitialized() {
            themeList["Light"] = _loc[nameof(RessourceAppStrings.light)];
            themeList["Dark"] = _loc[nameof(RessourceAppStrings.dark)];
        }

        /// <summary> Save the user-selection in the LocalStorage for remember by restart the PWA. </summary>
        private async void SaveUserThemePersistentAsync() {
            try {
                await LocalStorage.SetItemAsStringAsync("UserTheme", CurrentTheme);
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastColorThemePre)] + " " + CurrentTheme + " " +
                                            _loc[nameof(RessourceAppStrings.toastColorThemePost)], ToastLevel.Success);
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSaveError)], ToastLevel.Error);
            }
        }

        /// <summary>Save the Listform to localstorage persistant.</summary>
        /// <param name="value">new listwish bool</param>
        private async void WriteWishListToLocalStorage(bool value) {
            await LocalStorage.SetItemAsync<bool>("IsSingleWishListDashboard", value);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void WriteNoticableSideToLocalStorage(bool value) {
            await LocalStorage.SetItemAsync<bool>("NoticeableOnRightSide", value);
        }
        
        /// <summary></summary>
        /// <param name="value"></param>
        private async void WriteSendConfirmationMailToLocalStorage(U.ConfirmationMailMode value) {
            await LocalStorage.SetItemAsync<int>("ConfirmationMailMode", (int)value);
            await LocalStorage.RemoveItemAsync("SendConfirmationMail"); // alter Ja/Nein-Schluessel
            ToastService.ShowToast(_loc[nameof(RessourceAppStrings.confirmationMailSetting)] + ": " + MailModeText(value), ToastLevel.Success);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void SaveAmountOfPaymentsPersistentAsync(int value) {
            await LocalStorage.SetItemAsync<int>("loadAmountOfPayments", value);
            ToastService.ShowToast(value.ToString() + " " + _loc[nameof(RessourceAppStrings.toastSuccessfullPaymentAmount)], ToastLevel.Success);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void SaveAmountOfOrderPersistentAsync(int value) {
            await LocalStorage.SetItemAsync<int>("loadAmountOfOrders", value);
            ToastService.ShowToast(value.ToString() + " " + _loc[nameof(RessourceAppStrings.toastSuccessfullyOrderAmount)], ToastLevel.Success);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void SaveFilterShowProvider(bool value) {
            await LocalStorage.SetItemAsync<bool>("filterShowProvider", value);
            if (AppData.FilterShowProvider) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastFilterProviderOn)], ToastLevel.Success);
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastFilterProviderOff)], ToastLevel.Success);
            }
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void SaveFilterShowPerson(bool value) {
            await LocalStorage.SetItemAsync<bool>("filterShowPerson", value);
            if (AppData.FilterShowPerson) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastFilterPersonOn)], ToastLevel.Success);
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastFilterPersonOff)], ToastLevel.Success);
            }
        }

        /// <summary></summary>
        /// <param name="limitToSave"></param>
        private async void SaveDashboardMoneyLimit(Money limitToSave) {
            await LocalStorage.SetItemAsync<Money>("DashboardMoneyLimit", limitToSave);
            ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastLimitToValueSet)] + $" {limitToSave}", ToastLevel.Success);
        }

        /// <summary></summary>
        /// <returns></returns>
        private async Task RequestAccessTokenForChanges() {
            if (!await JsRuntime.InvokeAsync<bool>("confirm", "Mit diesem Token können Änderungen an bestehenden Datensätzen vollzogen werden." +
                "\n" +
                "HINWEIS: Zur Rückverfolgbarkeit wird jede Änderung inklusive des Initiators im Log der Essenskasse getrackt!")) {
                return;
            }
            
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "user/accessToken/get"),
                    Headers = {
                        { "sessionToken", await LocalStorage.GetItemAsync<string>("SessionToken") ?? "" },
                        { "name", await LocalStorage.GetItemAsync<string>("NameOfUser") ?? "" }
                    }
                };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                AppData.AccessToken = new Guid((await response.Content.ReadAsStringAsync())[1..^1]); // ()[1..^1] remove first and last character of String
                SaveAccessToken(AppData.AccessToken ?? new Guid("00000000-0000-0000-0000-000000000000"));
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        private async Task RequestNewAccessTokenForChanges() {
            if (await JsRuntime.InvokeAsync<bool>("confirm", "Sicher, daß Sie einen neuen beantragen wollen?")) {
                await RequestAccessTokenForChanges();
            }
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void SaveAccessToken(Guid value) {
            await LocalStorage.SetItemAsync<Guid>("AccessToken", value);
            ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSuccessRequestedAndSaved)], ToastLevel.Success);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void SaveAdminEmail(string value) {
            await LocalStorage.SetItemAsync<string>("AdminEmail", value);
            ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastAdminEmailEntered)] + "\n" + _loc[nameof(RessourceAppStrings.toastEventErrorRememberProcedurePlease)], ToastLevel.Success);
        }

        /// <summary></summary>
        /// <returns></returns>
        private async Task SendMessageToAdmin() {
            if (AdminEmail.Equals("")) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoEmailEntered)], ToastLevel.Info);
            }
            else {
                string subject = "Fehler in der \"Essenkasse\"";
                string body = "Bitte Namen, Zeit (Tag, Monat, Stunde, Minute) und Art des Fehlers schildern. " +
                    "Dies meint eine Beschreibung, wie es bei der Bedienung zu dem Fehler kam, um ihn schneller isolieren und beheben zu können." +
                    "\nDanke!";
                await JsRuntime.InvokeAsync<object>(
                    "blazorExtensions.SendLocalEmail",
                    new object[] {
                        AdminEmail,
                        subject,
                        body
                    }
                );
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        private async Task RequestOnlineSettingsForThisUser() {
            if (await JsRuntime.InvokeAsync<bool>("confirm", "Gespeicherte Einstellungen VOM Server abfragen und lokale überschreiben?")) {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "user/loadSettingsForUser"),
                    Headers = {
                        { "sessionToken", (await LocalStorage.GetItemAsync<Guid>("SessionToken")).ToString() },
                        { "name", await LocalStorage.GetItemAsync<string>("NameOfUser") },
                    }
                };

                HttpResponseMessage? response = null;
                try {
                    response = await HttpClient.SendAsync(httpRequestMessage);

                    U.Settings? backupSettings = JsonConvert.DeserializeObject<U.Settings>(await response.Content.ReadAsStringAsync());

                    if (backupSettings is null) {
                        ToastService.ShowToast(_loc[nameof(RessourceAppStrings.noSettingsAvailable)], ToastLevel.Warning);
                        return;
                    }
                    AppData.SettingsToAppData(backupSettings!);

                    //save to local storage
                    SaveAmountOfPaymentsPersistentAsync(AppData.LoadAmountPayment);
                    SaveAmountOfOrderPersistentAsync(AppData.LoadAmountOrder);
                    SaveFilterShowPerson(AppData.FilterShowPerson);
                    SaveFilterShowProvider(AppData.FilterShowProvider);
                    SaveDashboardMoneyLimit(AppData.DashboardMoneyLimit);
                    if (AppData.AccessToken is not null) {
                        SaveAccessToken(AppData.AccessToken ?? default);
                    }
                    SaveAdminEmail(AppData.AdminEmail);
                    WriteSendConfirmationMailToLocalStorage(AppData.SendConfirmationMail);

                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.settingsSuccsessfullyRestored)], ToastLevel.Success);
                }
                catch {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        private async Task SaveSettingsForThisUserOnline() {
            if (await JsRuntime.InvokeAsync<bool>("confirm", "Gespeicherte Einstellungen ZUM Server übertragen und online überschreiben?")) {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Config["API-Adress"] + "user/saveSettingsForUser"),
                    Headers = {
                        { "sessionToken", (await LocalStorage.GetItemAsync<Guid>("SessionToken")).ToString() },
                        { "name", await LocalStorage.GetItemAsync<string>("NameOfUser") },
                    },
                    Content = new StringContent(JsonConvert.SerializeObject(AppData.GetAppDataAsSettingsObject()), Encoding.UTF8, "application/json")
                };

                HttpResponseMessage? response = null;
                try {
                    response = await HttpClient.SendAsync(httpRequestMessage);
                }
                catch {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }

                if (response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.successfullyTransmitted)], ToastLevel.Success);
                }
                else {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
                }
            }
        }
        #endregion
    }
}
