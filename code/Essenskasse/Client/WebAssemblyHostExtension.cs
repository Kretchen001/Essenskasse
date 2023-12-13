using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

namespace Essenskasse.Client {

    public static class WebAssemblyHostExtensions {

        public async static Task SetDefaultCulture(this WebAssemblyHost host) {
            ILocalStorageService localStorage = host.Services.GetRequiredService<ILocalStorageService>();
            string cultureString = await localStorage.GetItemAsync<string>("culture");

            CultureInfo cultureInfo;

            if (!string.IsNullOrWhiteSpace(cultureString)) {
                cultureInfo = new CultureInfo(cultureString);
            }
            else {
                cultureInfo = new CultureInfo(LocalizerSettings.NeutralCulture.Name);
            }

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}