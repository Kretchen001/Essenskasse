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
        private bool IsASC = true;

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

        /// <summary>Gewaehlte Sortierung ("Asc" oder "Desc") fuer das Auswahlfeld; wird im Browser gemerkt.</summary>
        private string SortKey {
            get => IsASC ? "Asc" : "Desc";
            set {
                IsASC = value != "Desc";
                _ = SaveAscStatus();
                ApplyFilterAndSort();
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
                if (await LocalStorage.ContainKeyAsync("ProviderIsASC")) {
                    IsASC = await LocalStorage.GetItemAsync<bool>("ProviderIsASC");
                }
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
            string text = filter.Trim();
            IEnumerable<Provider> result = providers;
            if (text.Length > 0) {
                result = result.Where(x => x.Designation.Contains(text, StringComparison.CurrentCultureIgnoreCase));
            }
            shownProviderList = IsASC
                ? result.OrderBy(x => x.Designation, StringComparer.CurrentCultureIgnoreCase).ToList()
                : result.OrderByDescending(x => x.Designation, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        public async Task SaveAscStatus() {
            await LocalStorage.SetItemAsync<bool>("ProviderIsASC", IsASC);
        }
    }
}
