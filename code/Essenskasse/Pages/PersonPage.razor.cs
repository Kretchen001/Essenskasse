using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Essenskasse.Pages {
    
    public partial class PersonPage {

        private string dashboardLink = "/";
        private bool isAdding = false;
        private bool isLoading = true;
        private bool loadFailed = false;
        private bool showHidden = false;
        private List<Person> hiddenPersons = new List<Person>();
        private ElementReference newNameInput;
        private PersonSortMode sortMode = PersonSortMode.NameAsc;

        /// <summary>Gewaehlte Sortierung als Text fuer das Auswahlfeld; wird im Browser gemerkt.</summary>
        private string SortKey {
            get => sortMode.ToString();
            set {
                if (Enum.TryParse(value, out PersonSortMode mode)) {
                    sortMode = mode;
                    _ = UserSettings.SetAsync("PersonSort", mode.ToString());
                    ApplyFilterAndSort();
                }
            }
        }

        private string newPersonName = "";
        private string newPersonEmail = "";
        private string newPersonLanguage = LocalizerSettings.NeutralCulture.Culture;

        private string filter = "";

        /// <summary>Suchtext; jede Aenderung baut die Liste sofort neu auf.</summary>
        private string Filter {
            get => filter;
            set {
                filter = value ?? "";
                ApplyFilterAndSort();
            }
        }

        /// <summary>Gleiche Einteilung wie auf dem Dashboard: Saldo ueber dem Grenzwert ist unauffaellig.</summary>
        private bool IsInconspicuous(Person x) {
            return x.Saldo.IsGreater(AppData.DashboardMoneyLimit);
        }

        /// <summary>Esc leert die Suche.</summary>
        private void OnSearchKeyDown(KeyboardEventArgs e) {
            if (e.Key == "Escape" && Filter.Length > 0) {
                ClearFilter();
            }
        }

        private void ClearFilter() {
            Filter = "";
        }

        private PersonStatusFilter statusFilter = PersonStatusFilter.All;
        private static readonly PersonStatusFilter[] chipOrder = {
            PersonStatusFilter.All, PersonStatusFilter.Noticeable, PersonStatusFilter.Inconspicuous, PersonStatusFilter.NoEmail
        };

        private bool IsFiltered => filter.Trim().Length > 0 || statusFilter != PersonStatusFilter.All;

        private void SetStatusFilter(PersonStatusFilter value) {
            statusFilter = value;
            ApplyFilterAndSort();
        }

        private string ChipLabel(PersonStatusFilter chip) {
            return chip switch {
                PersonStatusFilter.Noticeable => _loc[nameof(RessourceAppStrings.noticeable)],
                PersonStatusFilter.Inconspicuous => _loc[nameof(RessourceAppStrings.inconspicuous)],
                PersonStatusFilter.NoEmail => _loc[nameof(RessourceAppStrings.filterNoEmail)],
                _ => _loc[nameof(RessourceAppStrings.filterAll)],
            };
        }

        /// <summary>Anzahl aller Personen je Schnellfilter (unabhaengig vom Suchtext).</summary>
        private int ChipCount(PersonStatusFilter chip) {
            return chip switch {
                PersonStatusFilter.Noticeable => person.Count(x => !IsInconspicuous(x)),
                PersonStatusFilter.Inconspicuous => person.Count(x => IsInconspicuous(x)),
                PersonStatusFilter.NoEmail => person.Count(x => x.Email.Length == 0),
                _ => person.Count,
            };
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
            isLoading = true;
            loadFailed = false;
            try {
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
                    loadFailed = true;
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
                    loadFailed = true;
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
                }
            }
            finally {
                isLoading = false;
            }
        }

        private async Task RetryLoadAsync() {
            await RequestAllPersonFromApiAsync();
        }

        /// <summary>Setzt Suchtext und Schnellfilter zurueck.</summary>
        private void ResetFilters() {
            filter = "";
            statusFilter = PersonStatusFilter.All;
            ApplyFilterAndSort();
        }

        private static readonly Regex EmailPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private bool NewNameEntered => newPersonName.Trim().Length > 0;
        private bool NewEmailInvalid => newPersonEmail.Trim().Length > 0 && !EmailPattern.IsMatch(newPersonEmail.Trim());
        private bool NewNameDuplicate => NewNameEntered
            && person.Any(x => string.Equals(x.Name, newPersonName.Trim(), StringComparison.CurrentCultureIgnoreCase));
        private bool CanAdd => NewNameEntered && !NewEmailInvalid && !isAdding;

        private async Task AddNewPerson() {
            if (!CanAdd) {
                return;
            }
            string name = newPersonName.Trim();
            string email = newPersonEmail.Trim();
            isAdding = true;
            try {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Config["API-Adress"] + "person/new"),
                    Headers = {
                        {"name", Uri.EscapeDataString(name)},
                        {"email", Uri.EscapeDataString(email)},
                        {"language", newPersonLanguage}
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
                    ApplyFilterAndSort();
                    ToastService.ShowToast(name + " " + _loc[nameof(RessourceAppStrings.toastAddedAndAvailable)], ToastLevel.Success);
                }
                else {
                    ToastService.ShowToast(name + " " + _loc[nameof(RessourceAppStrings.toastAddedButNotAvailable)], ToastLevel.Warning);
                }
                newPersonName = "";
                newPersonEmail = "";
            }
            finally {
                isAdding = false;
            }
            await newNameInput.FocusAsync(); // direkt die naechste Person eingeben
        }

        /// <summary>Eine Kachel hat die Person ausgeblendet: aus der Liste nehmen (und Liste der Ausgeblendeten auffrischen).</summary>
        private async Task OnPersonHidden(Person hidden) {
            person.RemoveAll(x => x.Guid == hidden.Guid);
            ApplyFilterAndSort();
            if (showHidden) {
                await LoadHiddenAsync();
            }
        }

        private async Task ToggleHiddenAsync() {
            showHidden = !showHidden;
            if (showHidden) {
                await LoadHiddenAsync();
            }
        }

        private async Task LoadHiddenAsync() {
            HttpRequestMessage request = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "person/requestHidden")
            };
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
                    return;
                }
                List<Person> loaded = JsonConvert.DeserializeObject<List<Person>>(await response.Content.ReadAsStringAsync()) ?? new List<Person>();
                loaded.ForEach(x => x.Name = Uri.UnescapeDataString(x.Name));
                loaded.ForEach(x => x.Email = Uri.UnescapeDataString(x.Email));
                hiddenPersons = loaded.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
            }
        }

        private async Task RestorePersonAsync(Person restored) {
            HttpRequestMessage request = new HttpRequestMessage() {
                Method = HttpMethod.Put,
                RequestUri = new Uri(Config["API-Adress"] + "person/restore"),
                Headers = { { "guid", restored.Guid.ToString() } }
            };
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
                    return;
                }
            }
            catch {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastConnectionError)], ToastLevel.Error);
                return;
            }
            hiddenPersons.RemoveAll(x => x.Guid == restored.Guid);
            person.Add(restored);
            ApplyFilterAndSort();
            ToastService.ShowToast(restored.Name + " " + _loc[nameof(RessourceAppStrings.toastPersonRestored)], ToastLevel.Success);
        }

        /// <summary>Eine Kachel hat Name oder E-Mail gespeichert: Liste (Filter, Sortierung, Zaehler) neu aufbauen.</summary>
        private void OnPersonUpdated(Person updated) {
            ApplyFilterAndSort();
        }

        private async Task ReadSortModeAsync() {
            string? stored = await UserSettings.GetAsync<string?>("PersonSort", null);
            if (Enum.TryParse(stored, out PersonSortMode mode)) {
                sortMode = mode;
            }
            else if (!await UserSettings.GetAsync("PersonIsASC", true)) {
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
            result = statusFilter switch {
                PersonStatusFilter.Noticeable => result.Where(x => !IsInconspicuous(x)),
                PersonStatusFilter.Inconspicuous => result.Where(x => IsInconspicuous(x)),
                PersonStatusFilter.NoEmail => result.Where(x => x.Email.Length == 0),
                _ => result,
            };
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
