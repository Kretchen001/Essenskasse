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
        private string currentUserName = string.Empty;
        private string nameToLogin = string.Empty;
        private string passwordToLogin = string.Empty;
        private const int DefaultRole = 10;
        private int roleToLogin = DefaultRole;
        private bool showPassword = false;
        private bool loginFailed = false;
        private bool isLoggingIn = false;

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected override async Task OnInitializedAsync() {
            AuthState.LoginRequested += () => {
                if (LoginModel is not null) {
                    ShowLoginModal();
                    StateHasChanged();
                }
            };
            await CheckLogin();
            await UserSettings.MigrateLegacyAsync();
            await CollectUserThemeAsync();
            await UserSettings.LoadAppDataAsync();
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
                        currentUserName = currentNameOfUser;
                        UserSettings.UseUser(currentNameOfUser); // ab jetzt gelten die Einstellungen dieses Benutzers
                        string newToken = await response.Content.ReadAsStringAsync();
                        if (newToken.Length > 0) {
                            // Die API antwortet mit einem JSON-String, also in Anfuehrungszeichen.
                            await LocalStorage.SetItemAsync<Guid>("SessionToken", new Guid(newToken.Trim('"')));
                        }
                    }
                    else {
                        AuthStateService.IsAuthenticated = false;
                        ToastService.ShowToast(_loc[nameof(RessourceAppStrings.loginFailedRetry)], ToastLevel.Error);
                    }
                    AuthState.NotifyAuthenticationStateChanged();
                }
                catch {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }
            }
        }

        /// <summary> Request the LocalStorage for the saved theme of the current user. If no one is found is the light-theme selected. </summary>
        private async Task CollectUserThemeAsync() {
            themeService.ThemeCode = await UserSettings.ReadThemeAsync();
        }

        private async Task LogoutUser() {
            await LocalStorage.RemoveItemAsync("NameOfUser");
            await LocalStorage.RemoveItemAsync("SessionToken");
            currentUserName = string.Empty;
            // Danach gelten wieder die geraeteweiten Einstellungen; der Aenderungstoken des Benutzers verschwindet aus dem Programm.
            UserSettings.UseUser("");
            await UserSettings.LoadAppDataAsync();
            await CollectUserThemeAsync();
            AuthStateService.IsAuthenticating = false;
            AuthStateService.IsAuthenticated = false;
            AuthState.NotifyAuthenticationStateChanged();
            ToastService.ShowToast(_loc[nameof(RessourceAppStrings.logout)], ToastLevel.Info);
        }

        #region oldCode - Remember
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
        #endregion

        /// <summary>Nach der Anmeldung: eigene lokale Einstellungen laden und einmal beim Server nachfragen.
        /// Kommt dort nichts (noch nie gespeichert oder nicht erreichbar), wird mit den lokalen bzw. Standardwerten weitergearbeitet.</summary>
        private async Task LoadUserSettingsAfterLoginAsync(string userName) {
            UserSettings.UseUser(userName);
            await UserSettings.MigrateLegacyAsync();
            await UserSettings.LoadAppDataAsync();
            await CollectUserThemeAsync();
            ServerSettingsResult result = await UserSettings.LoadFromServerAsync();
            if (result == ServerSettingsResult.AppliedReloadNeeded) {
                NavManager.NavigateTo(NavManager.Uri, true); // andere Sprache: einmal neu laden
            }
            else if (result == ServerSettingsResult.Applied) {
                await navMenu.ReloadNavMenu();
            }
        }

        private async Task LoginUser() {
            if (isLoggingIn || nameToLogin.Trim().Length == 0 || passwordToLogin.Length == 0) {
                return;
            }
            isLoggingIn = true;
            loginFailed = false;
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Config["API-Adress"] + "user/newTokenForUser"),
                Headers = {
                        { "name", nameToLogin },
                        { "pwd", passwordToLogin },
                        { "role", roleToLogin.ToString() }
                    }
            };
            HttpResponseMessage? response = null;
            try {
                response = await HttpClient.SendAsync(httpRequestMessage);
                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    AuthStateService.IsAuthenticated = true;
                    currentUserName = nameToLogin;
                    string newToken = await response.Content.ReadAsStringAsync();
                    await LocalStorage.SetItemAsync<string>("NameOfUser", nameToLogin);
                    await LocalStorage.SetItemAsync<Guid>("SessionToken", new Guid(newToken[1..(newToken.Length - 1)]));
                    string loggedInName = nameToLogin;
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.welcome)], ToastLevel.Success);
                    nameToLogin = "";
                    passwordToLogin = "";
                    roleToLogin = DefaultRole;
                    showPassword = false;
                    LoginModel?.Close();
                    await LoadUserSettingsAfterLoginAsync(loggedInName);
                }
                else {
                    // Dialog bleibt offen, das Passwort wird geleert und der Fokus springt zurueck ins Feld.
                    AuthStateService.IsAuthenticated = false;
                    loginFailed = true;
                    passwordToLogin = "";
                    if (LoginModel is not null) {
                        await LoginModel.FocusAsync("ek-login-password");
                    }
                }
                AuthState.NotifyAuthenticationStateChanged();
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
            finally {
                isLoggingIn = false;
            }
        }
    }
}
