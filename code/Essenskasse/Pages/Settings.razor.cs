using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;

namespace Essenskasse.Pages {

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
                AppData.LoadAmountPayment = value;
                SaveAmountOfPaymentsPersistentAsync(value);
            }
        }

        private int LoadAmountOrders {
            get => AppData.LoadAmountOrder;
            set {
                AppData.LoadAmountOrder = value;
                SaveAmountOfOrderPersistentAsync(value);
            }
        }

        private bool FilterShowProvider {
            get => AppData.FilterShowProvider;
            set {
                AppData.FilterShowProvider = value;
                SaveFilterShowProvider(value);
            }
        }

        private bool FilterShowPerson {
            get => AppData.FilterShowPerson;
            set {
                AppData.FilterShowPerson = value;
                SaveFilterShowPerson(value);
            }
        }

        private decimal DashboardMoneyLimitInt {
            get {
                decimal temp = AppData.DashboardMoneyLimit.Euro;
                temp += AppData.DashboardMoneyLimit.Cent * 0.01m;
                return temp;
            }
            set {
                if (value != 0) {
                    Money temp = ConvertToMoney(value);
                    if (temp.Cent < 100) {
                        DashboardMoneyLimit = temp;
                    }
                    else {
                        DashboardMoneyLimit = AppData.DashboardMoneyLimit;
                    }
                }
            }
        }
        private Money DashboardMoneyLimit {
            get => AppData.DashboardMoneyLimit;
            set {
                AppData.DashboardMoneyLimit = value;
                SaveDashboardMoneyLimit(value);
            }
        }

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

        /// <summary></summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static Money ConvertToMoney(decimal value) {
            long euro = (long)value;
            short cent = (short)((value - euro) * 100);
            return new Money(euro, cent);
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

        private async Task RequestAccessTokenForChanges() {
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "token/requestNewAccessToken")
                };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                Guid token = JsonConvert.DeserializeObject<Guid>(await response.Content.ReadAsStringAsync());
                AppData.AccessToken = token;
                SaveAccessToken(token);
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
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
        #endregion
    }
}
