using Essenskasse.Client;
using Essenskasse.Ressources;
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
                writeWishListToLocalStorage(value);
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
        private Dictionary<string, string> themeList = new Dictionary<string, string>() {
            { "Light", "Hell" },
            { "Dark", "Dunkel" }
        };

        /// <summary></summary>
        private int LoadAmountPayments {
            get => AppData.LoadAmountPayment;
            set {
                AppData.LoadAmountPayment = value;
                saveAmountOfPaymentsPersistentAsync(value);
            }
        }

        private int LoadAmountOrders {
            get => AppData.LoadAmountOrder;
            set {
                AppData.LoadAmountOrder = value;
                saveAmountOfOrderPersistentAsync(value);
            }
        }

        private bool FilterShowProvider {
            get => AppData.FilterShowProvider;
            set {
                AppData.FilterShowProvider = value;
                saveFilterShowProvider(value);
            }
        }

        private bool FilterShowPerson {
            get => AppData.FilterShowPerson;
            set {
                AppData.FilterShowPerson = value;
                saveFilterShowPerson(value);
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
                    Money temp = convertToMoney(value);
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
                saveDashboardMoneyLimit(value);
            }
        }

        private bool existAccessToken {
            get {
                if (AppData.AccessToken is not null) {
                    return true;
                }
                else {
                    return false;
                }
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
        private Money convertToMoney(decimal value) {
            long euro = (long)value;
            short cent = (short)((value - euro) * 100);
            return new Money(euro, cent);
        }

        /// <summary> Save the user-selection in the LocalStorage for remember by restart the PWA. </summary>
        private async void saveUserThemePersistentAsync() {
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
        private async void writeWishListToLocalStorage(bool value) {
            await LocalStorage.SetItemAsync<bool>("IsSingleWishListDashboard", value);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void saveAmountOfPaymentsPersistentAsync(int value) {
            await LocalStorage.SetItemAsync<int>("loadAmountOfPayments", value);
            ToastService.ShowToast(value.ToString() + " " + _loc[nameof(RessourceAppStrings.toastSuccessfullPaymentAmount)], ToastLevel.Success);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void saveAmountOfOrderPersistentAsync(int value) {
            await LocalStorage.SetItemAsync<int>("loadAmountOfOrders", value);
            ToastService.ShowToast(value.ToString() + " " + _loc[nameof(RessourceAppStrings.toastSuccessfullyOrderAmount)], ToastLevel.Success);
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void saveFilterShowProvider(bool value) {
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
        private async void saveFilterShowPerson(bool value) {
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
        private async void saveDashboardMoneyLimit(Money limitToSave) {
            await LocalStorage.SetItemAsync<Money>("DashboardMoneyLimit", limitToSave);
            ToastService.ShowToast($"Grenzwert auf {limitToSave.ToString()} gesetzt.", ToastLevel.Success);
        }

        private async Task requestAccessTokenForChanges() {
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "token/requestNewAccessToken")
                };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                Guid token = JsonConvert.DeserializeObject<Guid>(await response.Content.ReadAsStringAsync());
                AppData.AccessToken = token;
                saveAccessToken(token);
                base.StateHasChanged();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        /// <summary></summary>
        /// <param name="value"></param>
        private async void saveAccessToken(Guid value) {
            await LocalStorage.SetItemAsync<Guid>("AccessToken", value);
            ToastService.ShowToast("AccessToken erfolgreich abgerufen und eingetragen!", ToastLevel.Success);
        }
        #endregion
    }
}
