using Essenskasse.Client;
using Modells;

namespace Essenskasse.Layout {

    public partial class MainLayout {
        private NavMenu navMenu;

        protected override async Task OnInitializedAsync() {
            await CollectUserThemeAsync();
            await ReadLocalStorage();
            await navMenu.ReloadNavMenu();
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
    }
}
