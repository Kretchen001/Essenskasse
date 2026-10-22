using Microsoft.AspNetCore.Components;
using Essenskasse.Client;

namespace Essenskasse.Cards {

    public partial class CultureSelector {

        CultureWithName? SelectedCulture = null;

        protected override async Task OnInitializedAsync() {
            string? culture = await LocalStorage.GetItemAsync<string>("culture");
            if (culture == "") {
                SelectedCulture = LocalizerSettings.NeutralCulture;
            }
            else {
                SelectedCulture = (from x in LocalizerSettings.SupportedCultures
                                   where x.Culture == culture
                                   select x).FirstOrDefault();
            }
        }

        private async Task NewCultureSelectedAsync(ChangeEventArgs args) {
            string cultureString = args.Value!.ToString() ?? "de-DE";
            await LocalStorage.SetItemAsync<string>("culture", cultureString);
            Navi.NavigateTo(Navi.Uri, true);
        }
    }
}
