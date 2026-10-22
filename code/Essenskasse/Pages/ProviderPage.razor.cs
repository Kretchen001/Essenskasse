using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;

namespace Essenskasse.Pages {

    public partial class ProviderPage {

        private string dashboardLink = "/";
        private bool isAdding = false;
        private ElementReference newNameInput;
        private bool accessToDelete = false;
        private bool IsASC = true;

        private string newProviderDesignation = "";
        private string providerToDeleteGuid = "";

        private string filter = "";

        /// <summary>Suchtext; jede Aenderung baut die Liste sofort neu auf.</summary>
        private string Filter {
            get => filter;
            set {
                filter = value ?? "";
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
                IsASC = await LocalStorage.GetItemAsync<bool>("ProviderIsASC");
                await RequestAllProviderFromApiAsync();
            }
            base.StateHasChanged();
        }

        private async Task RequestAllProviderFromApiAsync() {
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
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }

            if (response.IsSuccessStatusCode) { // success?
                                                // deserialize the result-JSON with Newtownsoft to the List
                providers = JsonConvert.DeserializeObject<List<Provider>>(await response.Content.ReadAsStringAsync()) ?? new List<Provider>();
                providers.ForEach(x => x.Designation = Uri.UnescapeDataString(x.Designation));
                ApplyFilterAndSort();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
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

        private async Task DeleteAnbieter() {
            if (Guid.TryParse(providerToDeleteGuid, out Guid providerDesignation)) {
                Provider? tempAnbieter = providers.Find(x => x.Guid.Equals(new Guid(providerToDeleteGuid)));
                if (tempAnbieter is null) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastGuidNotThere)], ToastLevel.Info);
                    return;
                }
                if (await JSRuntime.InvokeAsync<bool>(
                                    "confirm",
                                    "Wollen sie den Anbieter - " +
                                    tempAnbieter.Designation +
                                    " - wirklich löschen?")) {
                    // Bestätigt, also lösche
                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri(Config["API-Adress"] + "provider/remove"),
                        Headers = {
                            {"guid", providerToDeleteGuid}
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

                    if (response.IsSuccessStatusCode) {
                        Provider temp = JsonConvert.DeserializeObject<Provider>(await response.Content.ReadAsStringAsync())!;
                        ToastService.ShowToast(_loc[nameof(RessourceAppStrings.person)] + " " + temp.Designation + " " + _loc[nameof(RessourceAppStrings.toastSuccessfullyRemoved)], ToastLevel.Success);
                        providers.Remove(tempAnbieter);
                        ApplyFilterAndSort();
                        accessToDelete = false;
                        providerToDeleteGuid = "";
                        base.StateHasChanged();
                    }
                    else if (response.StatusCode.Equals(System.Net.HttpStatusCode.NoContent)) {
                        ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastRemoveNotBecauseNoEntity)], ToastLevel.Info);
                    }
                    else {
                        ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoRemove)] + "\n" + _loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
                    }
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoGuidThere)], ToastLevel.Error);
            }
        }

        private void ToggleAscDesc() {
            IsASC = !IsASC;
            _ = SaveAscStatus();
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
