using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;

namespace Essenskasse.Pages {

    public partial class ProviderPage {

        private string dashboardLink = "/";
        private bool accessToAdd = false;
        private bool accessToDelete = false;
        private bool IsASC = true;

        private string newProviderDesignation = "";
        private string providerToDeleteGuid = "";

        private string filter = "";

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
                providers = providers.OrderBy(x => x.Designation).ToList();
                shownProviderList.AddRange(providers);
                SortProviderList();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async void AddNewAnbieter() {
            if (newProviderDesignation.Equals("")) {
                return;
            }
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Config["API-Adress"] + "provider/new"),
                Headers = {
                        {"name", Uri.EscapeDataString(newProviderDesignation)}
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
                string temp = await response.Content.ReadAsStringAsync();
                providers.Add(new Provider(new Guid(temp[1..(temp.Length - 1)]), newProviderDesignation));
                providers = providers.OrderBy(x => x.Designation).ToList();
                shownProviderList.Add(new Provider(new Guid(temp[1..(temp.Length - 1)]), newProviderDesignation));
                SortProviderList();
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.provider)] + " " + newProviderDesignation + " " + _loc[nameof(RessourceAppStrings.toastAdd)], ToastLevel.Success);
            }

            newProviderDesignation = "";
            accessToAdd = false;
            base.StateHasChanged();
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

        private async Task CopyToClipboard(string text) {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }

        private void ChangeProviderShownList() {
            Console.WriteLine(filter);
            shownProviderList.Clear();
            if (filter.Equals("")) {
                shownProviderList.AddRange(providers);
            }
            else {
                foreach (Provider x in providers) {
                    if (x.Designation.Contains(filter)) {
                        shownProviderList.Add(x);
                    }
                }
            }
        }

        private void ToggleAscDesc() {
            IsASC = !IsASC;
            _ = SaveAscStatus();
            SortProviderList();
            base.StateHasChanged();
        }

        private void SortProviderList() {
            if (IsASC) {
                shownProviderList = shownProviderList.OrderBy(x => x.Designation).ToList();
            }
            else {
                shownProviderList = shownProviderList.OrderByDescending(x => x.Designation).ToList();
            }
        }

        public async Task SaveAscStatus() {
            await LocalStorage.SetItemAsync<bool>("ProviderIsASC", IsASC);
        }
    }
}
