using Microsoft.AspNetCore.Components;
using Essenskasse.Client;
using Essenskasse.Ressources;
using Modells;
using Newtonsoft.Json;

namespace Essenskasse.Cards {

    public partial class EditPersonCard {

        /// <summary></summary>
        [Parameter]
        public EventCallback OnChangeCommit { get; set; }

        /// <summary></summary>
        [Parameter]
        public Person person { get; set; } = new Person();
        private Person safePerson = new Person();

        private string selectedPerson = "";

        private bool dataNotLoadYet = true;

        private async void requestPersonFromTheGuidInput () {
            if (Guid.TryParse(selectedPerson, out _)) {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config["API-Adress"] + "person/perGuid"),
                    Headers = {
                        { "guid", Guid.Parse(selectedPerson).ToString() }
                    }
                };

                HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

                if (response.IsSuccessStatusCode) {
                    person = JsonConvert.DeserializeObject<Person>(await response.Content.ReadAsStringAsync())!;
                    person.Name = Uri.UnescapeDataString(person.Name);
                    person.Email= Uri.UnescapeDataString(person.Email);
                    safePerson = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(person))!;
                    dataNotLoadYet = false;
                    base.StateHasChanged();
                }
                else {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastPersonNotAvailableCheckGuid)], ToastLevel.Warning);
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoGuidThere)], ToastLevel.Error);
            }
        }

        private async void commitChangesToApi() {
            if (person.Name.Equals(safePerson.Name) && person.Email.Equals(safePerson.Email)) {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoChanges)], ToastLevel.Info);
                return;
            }

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(Config["API-Adress"] + "person/update"),
                    Headers = {
                        { "guid", Guid.Parse(selectedPerson).ToString() },
                        { "name", person.Name },
                        { "email", person.Email }
                    }
                };

            HttpResponseMessage response = await HttpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode) {
                safePerson = new Person(new Guid(), "", "", new Money());
                person = new Person(new Guid(), "", "", new Money());
                dataNotLoadYet = true;
                ToastService.ShowToast("Erfolgreich geändert!", ToastLevel.Success);
                await OnChangeCommit.InvokeAsync();
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastPersonNotAvailableCheckGuid)], ToastLevel.Warning);
            }
        }
    }
}
