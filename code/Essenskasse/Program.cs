using Blazored.LocalStorage;
using Essenskasse.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

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
            builder.Services.AddBlazoredLocalStorage();

            //await builder.Build().RunAsync(); // <- Old Version
            WebAssemblyHost host = builder.Build();
            await host.SetDefaultCulture();
            await host.RunAsync();
        }
    }
}
