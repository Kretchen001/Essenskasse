using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;

namespace Essenskasse.Pages {

    public partial class ProviderPage {

        private string dashboardLink = "/";
        private bool isAdding = false;
        private ElementReference newNameInput;
        private bool isLoading = true;
        private bool loadFailed = false;
        private ProviderSort sortMode = ProviderSort.NameAsc;
        private Dictionary<Guid, ProviderStatistics> statistics = new Dictionary<Guid, ProviderStatistics>();

        private string newProviderDesignation = "";

        private string filter = "";

        /// <summary>Suchtext; jede Aenderung baut die Liste sofort neu auf.</summary>
        private string Filter {
            get => filter;
            set {
                filter = value ?? "";
                ApplyFilterAndSort();
            }
        }

        /// <summary>Esc leert die Suche.</summary>
        private void OnSearchKeyDown(KeyboardEventArgs e) {
            if (e.Key == "Escape" && Filter.Length > 0) {
                ClearFilter();
            }
        }

        /// <summary>Gewaehlte Sortierung als Text fuer das Auswahlfeld; wird im Browser gemerkt.</summary>
        private string SortKey {
            get => sortMode.ToString();
            set {
                if (Enum.TryParse(value, out ProviderSort mode)) {
                    sortMode = mode;
                    _ = UserSettings.SetAsync("ProviderSort", mode.ToString());
                    ApplyFilterAndSort();
                }
            }
        }

        private ProviderStatistics? StatisticsOf(Provider provider) {
            return statistics.TryGetValue(provider.Guid, out ProviderStatistics? s) ? s : null;
        }

        private async Task ReadSortModeAsync() {
            string? stored = await UserSettings.GetAsync<string?>("ProviderSort", null);
            if (Enum.TryParse(stored, out ProviderSort mode)) {
                sortMode = mode;
            }
            else if (!await UserSettings.GetAsync("ProviderIsASC", true)) {
                sortMode = ProviderSort.NameDesc; // alte Einstellung (absteigend) uebernehmen
            }
        }

        private void ClearFilter() {
            Filter = "";
        }

        private List<Provider> providers = new List<Provider>();
        private List<Provider> shownProviderList = new List<Provider>();

        protected override async Task OnInitializedAsync() {
            if (!AuthStateService.IsAuthenticated) {
                Nav.NavigateTo("/");
            }
            else {
                await ReadSortModeAsync();
                await RequestAllProviderFromApiAsync();
            }
            base.StateHasChanged();
        }

        private async Task RequestAllProviderFromApiAsync() {
            isLoading = true;
            loadFailed = false;
            try {
                // Request all providers, that are saved in the database.
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "provider/requestAll")
                };

                HttpResponseMessage? response = null;
                try {
                    response = await HttpClient.SendAsync(httpRequestMessage);
                }
                catch {
                    loadFailed = true;
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }

                if (response.IsSuccessStatusCode) {
                    providers = JsonConvert.DeserializeObject<List<Provider>>(await response.Content.ReadAsStringAsync()) ?? new List<Provider>();
                    providers.ForEach(x => x.Designation = Uri.UnescapeDataString(x.Designation));
                    ApplyFilterAndSort();
                    await RequestStatisticsAsync();
                }
                else {
                    loadFailed = true;
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
                }
            }
            finally {
                isLoading = false;
            }
        }

        /// <summary>Laedt Anzahl, Summe und letzte Bestellung je Anbieter. Ohne diese Daten funktioniert die Seite weiter.</summary>
        private async Task RequestStatisticsAsync() {
            HttpRequestMessage request = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "provider/statistics")
            };
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    List<ProviderStatistics> loaded = JsonConvert.DeserializeObject<List<ProviderStatistics>>(await response.Content.ReadAsStringAsync()) ?? new List<ProviderStatistics>();
                    statistics = loaded.ToDictionary(x => x.ProviderGuid);
                    ApplyFilterAndSort();
                }
            }
            catch {
                // Auswertung ist optional.
            }
        }

        private bool NewNameEntered => newProviderDesignation.Trim().Length > 0;
        private bool NewNameDuplicate => NewNameEntered
            && providers.Any(x => string.Equals(x.Designation, newProviderDesignation.Trim(), StringComparison.CurrentCultureIgnoreCase));
        private bool CanAdd => NewNameEntered && !isAdding;

        private async Task AddNewProvider() {
            if (!CanAdd) {
                return;
            }
            string name = newProviderDesignation.Trim();
            isAdding = true;
            try {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Config["API-Adress"] + "provider/new"),
                    Headers = {
                        {"name", Uri.EscapeDataString(name)}
                    }
                };

                HttpResponseMessage? response = null;
                try {
                    response = await HttpClient.SendAsync(httpRequestMessage);
                }
                catch {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                    return;
                }

                if (!response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
                    return;
                }
                string temp = await response.Content.ReadAsStringAsync();
                providers.Add(new Provider(new Guid(temp[1..(temp.Length - 1)]), name));
                ApplyFilterAndSort();
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.provider)] + " " + name + " " + _loc[nameof(RessourceAppStrings.toastAdd)], ToastLevel.Success);
                newProviderDesignation = "";
            }
            finally {
                isAdding = false;
            }
            await newNameInput.FocusAsync(); // direkt den naechsten Anbieter eingeben
        }

        /// <summary>Eine Kachel hat die Bezeichnung gespeichert: Liste (Filter, Sortierung) neu aufbauen.</summary>
        private void OnProviderUpdated(Provider updated) {
            ApplyFilterAndSort();
        }

        /// <summary>Eine Kachel hat den Anbieter entfernt: aus der Liste nehmen.</summary>
        private void OnProviderRemoved(Provider removed) {
            providers.RemoveAll(x => x.Guid == removed.Guid);
            ApplyFilterAndSort();
        }

        /// <summary>Baut die angezeigte Liste aus allen Anbietern: Suchtext (ohne Beachtung der Schreibweise), dann Sortierung.</summary>
        private void ApplyFilterAndSort() {
            shownProviderList = ProviderListing.Apply(providers, filter, sortMode, statistics);
        }
    }
}
