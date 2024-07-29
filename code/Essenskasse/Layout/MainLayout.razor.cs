using Essenskasse.Client;
using Essenskasse.Client.Services;
using Essenskasse.Ressources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Modells;

namespace Essenskasse.Layout {

    public partial class MainLayout {

        private NavMenu navMenu;

        private LoginModal? LoginModel { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected override async Task OnInitializedAsync() {
            await CheckLogin();
            await CollectUserThemeAsync();
            await ReadLocalStorage();
            await navMenu.ReloadNavMenu();
        }

        private void ShowLoginModal() {
            LoginModel!.Open();
        }

        private async Task CheckLogin() {
            Guid currentSessionToken = await LocalStorage.GetItemAsync<Guid>("SessionToken");
            string currentNameOfUser = await LocalStorage.GetItemAsync<string>("NameOfUser") ?? "";

            if (currentNameOfUser != "" && !currentSessionToken.Equals(new Guid())) {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "user/checkTokenForUser"),
                    Headers = {
                        { "name", currentNameOfUser },
                        { "sessionToken", currentSessionToken.ToString() }
                    }
                };
                HttpResponseMessage? response = null;
                try {
                    response = await HttpClient.SendAsync(httpRequestMessage);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                        AuthStateService.IsAuthenticated = true;
                        string newToken = await response.Content.ReadAsStringAsync();
                        if (newToken.Length > 0) {
                            await LocalStorage.SetItemAsync<Guid>("SessionToken", new Guid(newToken));
                        }
                    }
                    else {
                        AuthStateService.IsAuthenticated = false;
                        ToastService.ShowToast("Anmeldung fehlgeschlagen, bitte Anmeldedaten neu eingeben!", ToastLevel.Error);
                    }

                    AuthState.NotifyAuthenticationStateChanged();
                }
                catch {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }
            }
        }

        /// <summary> Request the LocalStorage for a saved theme. If no one is found is the light-theme selected. </summary>
        private async Task CollectUserThemeAsync() {
            switch (await LocalStorage.GetItemAsStringAsync("UserTheme")) {
                case "Dark": {
                        themeService.ThemeCode = ColorCode.Dark;
                        break;
                    }
                default: { // ^= light-theme
                        themeService.ThemeCode = ColorCode.Light;
                        break;
                    }
            }
        }

        /// <summary>Read all Key-Value Pairs from the LocalStorage and bring it to its necessary place.</summary>
        private async Task ReadLocalStorage() {
            AppData.IsSingleListWishDashboard = await LocalStorage.GetItemAsync<bool>("IsSingleWishListDashboard");
            AppData.NoticeableOnRightSide = await LocalStorage.GetItemAsync<bool>("NoticeableOnRightSide");
            AppData.LoadAmountPayment = await LocalStorage.GetItemAsync<int>("loadAmountOfPayments") == 0 ? 10 : await LocalStorage.GetItemAsync<int>("loadAmountOfPayments");
            AppData.LoadAmountOrder = await LocalStorage.GetItemAsync<int>("loadAmountOfOrders") == 0 ? 25 : await LocalStorage.GetItemAsync<int>("loadAmountOfOrders");
            AppData.FilterShowProvider = await LocalStorage.GetItemAsync<bool>("filterShowProvider");
            AppData.FilterShowPerson = await LocalStorage.GetItemAsync<bool>("filterShowPerson");
            AppData.AccessToken = await LocalStorage.GetItemAsync<Guid>("AccessToken");
            AppData.DashboardMoneyLimit = await LocalStorage.GetItemAsync<Money>("DashboardMoneyLimit") ?? new Money(0, 0);
            if (AppData.AccessToken.Equals(new Guid("00000000-0000-0000-0000-000000000000"))) {
                AppData.AccessToken = null;
            }
            AppData.AdminEmail = await LocalStorage.GetItemAsync<string>("AdminEmail") ?? "";
        }

        private void UpdateAuthentication(bool? isAuthenticated) {
            if (!isAuthenticated.HasValue) {
                AuthStateService.IsAuthenticating = true;
            }
            else {
                AuthStateService.IsAuthenticating = false;
                AuthStateService.IsAuthenticated = isAuthenticated.Value;
            }

            AuthState.NotifyAuthenticationStateChanged();
        }
    }
}
