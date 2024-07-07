using Blazored.LocalStorage;
using Essenskasse.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json.Serialization;
using System.Text.Json;
using Essenskasse.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Essenskasse {
    public class Program {
        public static async Task Main(string[] args) {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            // access to Localization
            builder.Services.AddLocalization(options => {
                options.ResourcesPath = "Ressources";
            });

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

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
            
            // add access to the toast's
            builder.Services.AddScoped<ToastService>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthStateProvivder>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<AuthStateProvivder>());

            //await builder.Build().RunAsync(); // <- Old Version
            WebAssemblyHost host = builder.Build();
            await host.SetDefaultCulture();
            await host.RunAsync();
        }
    }
}
