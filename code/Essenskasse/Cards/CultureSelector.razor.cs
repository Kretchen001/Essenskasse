using Microsoft.AspNetCore.Components;
using Essenskasse.Client;
using Essenskasse.Client.Localization;

namespace Essenskasse.Cards {

    public partial class CultureSelector {

        CultureWithName? SelectedCulture = null;

        protected override async Task OnInitializedAsync() {
            _ = Texts.LoadAllAsync(); // die uebrigen Sprachen im Hintergrund holen, damit der Wechsel sofort geht
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
            await Language.SetAsync(cultureString); // wechselt sofort, die Seite wird nicht neu geladen
        }
    }
}
