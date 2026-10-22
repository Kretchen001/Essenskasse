using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;

namespace Essenskasse.Pages {
    
    public partial class PersonPage {

        private string dashboardLink = "/";
        private bool accessToAdd = false;
        private PersonSortMode sortMode = PersonSortMode.NameAsc;

        /// <summary>Gewaehlte Sortierung als Text fuer das Auswahlfeld; wird im Browser gemerkt.</summary>
        private string SortKey {
            get => sortMode.ToString();
            set {
                if (Enum.TryParse(value, out PersonSortMode mode)) {
                    sortMode = mode;
                    _ = LocalStorage.SetItemAsync<string>("PersonSort", mode.ToString());
                    ApplyFilterAndSort();
                }
            }
        }

        private string newPersonName = "";
        private string newPersonEmail = "";

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

        private List<Person> person = new List<Person>();
        private List<Person> shownPersonList = new List<Person>();

        protected override async Task OnInitializedAsync() {
            if (!AuthStateService.IsAuthenticated) {
                Nav.NavigateTo("/");
            }
            else {
                await ReadSortModeAsync();
                await RequestAllPersonFromApiAsync();
            }
            base.StateHasChanged();
        }

        private async Task RequestAllPersonFromApiAsync() {
            // Request all Person, that are saved in the database
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "person/requestAll")
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
                person = JsonConvert.DeserializeObject<List<Person>>(await response.Content.ReadAsStringAsync()) ?? new List<Person>();
                person.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                person.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
                person = person.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
                ApplyFilterAndSort();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async void AddNewPerson() {
            if (newPersonName.Equals("")) {
                return;
            }
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Config["API-Adress"] + "person/new"),
                Headers = {
                        {"name", Uri.EscapeDataString(newPersonName)},
                        {"email", Uri.EscapeDataString(newPersonEmail)}
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
                string tempGuid = await response.Content.ReadAsStringAsync();
                HttpRequestMessage httpRequestMessage2 = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "person/perGuid"),
                    Headers = {
                        {"guid", tempGuid[1..(tempGuid.Length - 1)] }
                    }
                };

                HttpResponseMessage response2 = await HttpClient.SendAsync(httpRequestMessage2);

                if (response2.IsSuccessStatusCode) {
                    Person temp = JsonConvert.DeserializeObject<Person>(await response2.Content.ReadAsStringAsync())!;
                    temp.Name = Uri.UnescapeDataString(temp.Name);
                    temp.Email = Uri.UnescapeDataString(temp.Email);
                    person.Add(temp);
                    ToastService.ShowToast(newPersonName + " " + _loc[nameof(RessourceAppStrings.toastAddedAndAvailable)], ToastLevel.Success);
                    accessToAdd = false;
                    newPersonName = newPersonEmail = ""; 
                    ApplyFilterAndSort();
                    base.StateHasChanged();
                }
                else {
                    ToastService.ShowToast(newPersonName + " " + _loc[nameof(RessourceAppStrings.toastAddedButNotAvailable)], ToastLevel.Warning);
                    accessToAdd = false;
                    newPersonName = "";
                    newPersonEmail = "";
                    base.StateHasChanged();
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async void ChangeOfPersonCommit() {
            person.Clear();
            shownPersonList.Clear();
            await RequestAllPersonFromApiAsync();
            base.StateHasChanged();
        }

        private async Task ReadSortModeAsync() {
            string? stored = await LocalStorage.GetItemAsync<string>("PersonSort");
            if (Enum.TryParse(stored, out PersonSortMode mode)) {
                sortMode = mode;
            }
            else if (await LocalStorage.ContainKeyAsync("PersonIsASC") && !await LocalStorage.GetItemAsync<bool>("PersonIsASC")) {
                sortMode = PersonSortMode.NameDesc; // alte Einstellung (absteigend) uebernehmen
            }
        }

        /// <summary>Baut die angezeigte Liste immer aus allen Personen, dem aktuellen Filter (Name oder E-Mail, ohne Beachtung der
        /// Schreibweise) und der Sortierung neu auf. Alle Stellen, die die Liste aendern, rufen das auf.</summary>
        private void ApplyFilterAndSort() {
            IEnumerable<Person> result = person;
            string text = filter.Trim();
            if (text.Length > 0) {
                result = result.Where(x => x.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase)
                                        || x.Email.Contains(text, StringComparison.CurrentCultureIgnoreCase));
            }
            StringComparer byName = StringComparer.CurrentCultureIgnoreCase;
            shownPersonList = (sortMode switch {
                PersonSortMode.NameDesc => result.OrderByDescending(x => x.Name, byName),
                PersonSortMode.SaldoAsc => result.OrderBy(x => x.Saldo.ToTotalCents()).ThenBy(x => x.Name, byName),
                PersonSortMode.SaldoDesc => result.OrderByDescending(x => x.Saldo.ToTotalCents()).ThenBy(x => x.Name, byName),
                PersonSortMode.NoEmailFirst => result.OrderBy(x => x.Email.Length > 0).ThenBy(x => x.Name, byName),
                _ => result.OrderBy(x => x.Name, byName),
            }).ToList();
        }

    }
}
