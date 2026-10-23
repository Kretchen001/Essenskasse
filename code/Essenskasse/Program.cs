using Blazored.LocalStorage;
using Essenskasse.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json.Serialization;
using System.Text.Json;
using Essenskasse.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Essenskasse.Client.Localization;

namespace Essenskasse {
    public class Program {
        public static async Task Main(string[] args) {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Texte liegen als JSON unter wwwroot/i18n: beim Start wird nur die Startsprache geladen, die anderen erst beim Oeffnen
            // der Einstellungen, damit die Sprache ohne Neuladen gewechselt werden kann (IStringLocalizer<T> liefert immer die aktuelle Sprache)
            TextService texts = new(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton(texts);
            builder.Services.AddSingleton(typeof(IStringLocalizer<>), typeof(JsonStringLocalizer<>));
            builder.Services.AddScoped<LanguageService>();

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // HttpClient, der an jede API-Anfrage das Sitzungs-Token des angemeldeten Benutzers haengt
            builder.Services.AddScoped(sp => new HttpClient(new SessionTokenHandler(
                    sp.GetRequiredService<ILocalStorageService>(),
                    sp.GetRequiredService<IConfiguration>()) {
                InnerHandler = new HttpClientHandler()
            }) {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            // access to LocalStorage
            builder.Services.AddBlazoredLocalStorage(config => {
                config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                config.JsonSerializerOptions.WriteIndented = false;
            });

            // add the AppData as Singelton, so by injection the data is everywhere available in the program
            builder.Services.AddSingleton<AppDataService>();

            // add the ThemeService as a Simgleton to access it by injection
            builder.Services.AddSingleton<ThemeService>();

            // Einstellungen im Browser, getrennt pro angemeldetem Benutzer (plus Abgleich mit dem Server)
            builder.Services.AddScoped<UserSettingsStore>();
            
            // add access to the toast's
            builder.Services.AddScoped<ToastService>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthStateService>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<AuthStateService>());

            //await builder.Build().RunAsync(); // <- Old Version
            WebAssemblyHost host = builder.Build();
            await host.SetDefaultCulture();
            await texts.LoadStartAsync(); // nur die Startsprache; die anderen folgen beim Oeffnen der Einstellungen
            await host.RunAsync();
        }
    }
}
