using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;

namespace Essenskasse.Pages {
    
    public partial class PersonPage {

        private bool accessToAdd = false;
        private bool IsASC = true;

        private string newPersonName = "";
        private string newPersonEmail = "";

        private string filter = "";

        private List<Person> person = new List<Person>();
        private List<Person> shownPersonList = new List<Person>();

        protected override async Task OnInitializedAsync() {
            if (!AuthStateService.IsAuthenticated) {
                Nav.NavigateTo("/");
            }
            else {
                IsASC = await LocalStorage.GetItemAsync<bool>("PersonIsASC");
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
                person = person.OrderBy(x => x.Name).ToList();
                shownPersonList.AddRange(person);
                SortPersonList();
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
                    shownPersonList.Add(temp);
                    ToastService.ShowToast(newPersonName + " " + _loc[nameof(RessourceAppStrings.toastAddedAndAvailable)], ToastLevel.Success);
                    accessToAdd = false;
                    newPersonName = newPersonEmail = ""; 
                    person = person.OrderBy(x => x.Name).ToList();
                    SortPersonList();
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

        public async Task CopyToClipboard(string text) {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }

        private void ChangePersonShownList() {
            Console.WriteLine(filter);
            shownPersonList.Clear();
            if (filter.Equals("")) {
                shownPersonList.AddRange(person);
            }
            else {
                foreach (Person x in person) {
                    if (x.Name.Contains(filter)) {
                        shownPersonList.Add(x);
                    }
                }
            }
        }

        private void ToggleAscDesc() {
            IsASC = !IsASC;
            _ = SaveAscStatus();
            SortPersonList();
            base.StateHasChanged();
        }

        private void SortPersonList() {
            if (IsASC) {
                shownPersonList = shownPersonList.OrderBy(x => x.Name).ToList();
            }
            else {
                shownPersonList = shownPersonList.OrderByDescending(x => x.Name).ToList();
            }
        }

        public async Task SaveAscStatus() {
            await LocalStorage.SetItemAsync<bool>("PersonIsASC", IsASC);
        }
    }
}
