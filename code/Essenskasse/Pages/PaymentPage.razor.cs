using Essenskasse.Client;
using Essenskasse.Ressources;
using Microsoft.JSInterop;
using Modells;
using Newtonsoft.Json;
using System.Text;

namespace Essenskasse.Pages {

    public partial class PaymentPage {

        private string dashboardLink = "/";
        private List<Person>? person = new List<Person>();
        private Guid _selectedPerson;
        private Guid SelectedPerson {
            get {
                return _selectedPerson;
            }
            set {
                _selectedPerson = value;
                previewOpen = false;
                _ = RequestPaymentsOfPerson();
            }
        }

        /// <summary>Eingetippter Betrag, z. B. "12,50". Wird erst beim Buchen in einen Geldbetrag umgewandelt.</summary>
        private string _amountText = "";
        private string amountText {
            get => _amountText;
            set {
                _amountText = value ?? "";
                previewOpen = false; // die Vorschau gilt nur fuer genau diesen Betrag
            }
        }
        private bool AmountEntered => amountText.Trim().Length > 0;
        private bool AmountValid => MoneyParser.TryParse(amountText, out Money parsed) && parsed.ToTotalCents() != 0;
        private Money Money => MoneyParser.TryParse(amountText, out Money parsed) ? parsed : new Money(0, 0);

        private string comment = "";

        private List<Payment> paymentsOfSelectedPerson = new List<Payment>();
        private int Amount {
            get => AppData.LoadAmountPayment;
        }

        protected override async Task OnInitializedAsync() {
            if (!AuthStateService.IsAuthenticated) {
                Nav.NavigateTo("/");
            }
            else {
                await RefreshPersonList();
            }
            base.StateHasChanged();
        }

        private async Task RefreshPersonList() {
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

                if (person.Count > 0) {
                    SelectedPerson = person[0].Guid;
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoApiConnection)], ToastLevel.Warning);
            }
        }

        private async Task RequestPaymentsOfPerson() {
            // Request last payments of a person
            Guid requestedFor = SelectedPerson;
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "payment/fromPerson"),
                Headers = {
                    { "guid", requestedFor.ToString() },
                    { "amount", Amount.ToString() }
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
                List<Payment> loaded = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();
                if (requestedFor != SelectedPerson) {
                    return; // zwischenzeitlich wurde eine andere Person gewaehlt, deren Liste nicht ueberschreiben
                }
                paymentsOfSelectedPerson = loaded;
                paymentsOfSelectedPerson.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment));
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }

            base.StateHasChanged();
        }

        private bool isBooking = false;
        private bool previewOpen = false;

        private Person? SelectedPersonData => person?.Find(x => x.Guid.Equals(SelectedPerson));

        /// <summary>Stand der Person nach der Buchung (Stand jetzt plus Betrag).</summary>
        private Money? BalanceAfter => SelectedPersonData is null
            ? null
            : Money.FromTotalCents(SelectedPersonData.Saldo.ToTotalCents() + Money.ToTotalCents());

        private void OpenPreview() {
            if (AmountValid && SelectedPersonData is not null) {
                previewOpen = true;
            }
        }

        private void ClosePreview() {
            previewOpen = false;
        }

        /// <summary>Holt den aktuellen Stand der Person vom Server, damit die naechste Vorschau stimmt.</summary>
        private async Task RefreshSaldoAsync(Guid guid) {
            HttpRequestMessage request = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "person/perGuid"),
                Headers = { { "guid", guid.ToString() } }
            };
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    Person? fresh = JsonConvert.DeserializeObject<Person>(await response.Content.ReadAsStringAsync());
                    Person? local = person?.Find(x => x.Guid.Equals(guid));
                    if (fresh is not null && local is not null) {
                        local.Saldo = fresh.Saldo;
                    }
                }
            }
            catch {
                // Der Stand ist nur Anzeige; ein Fehler hier darf die Buchung nicht beeintraechtigen.
            }
        }

        /// <summary>Bucht die Einzahlung. Solange eine Buchung laeuft, wird jeder weitere Aufruf ignoriert (kein doppeltes Buchen).</summary>
        private async Task InsertPayment() {
            if (isBooking) {
                return;
            }
            isBooking = true;
            try {
                await InsertPaymentCoreAsync();
            }
            finally {
                isBooking = false;
                previewOpen = false;
            }
        }

        private async Task InsertPaymentCoreAsync() {
            if (!AmountValid) {
                return;
            }
            Guid bookedFor = SelectedPerson;
            Money booked = Money;
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Config["API-Adress"] + "payment/insertNew"),
                Headers = {
                    {"referencedPerson", bookedFor.ToString()},
                    {"comment", Uri.EscapeDataString(comment)}
                },
                Content = new StringContent(JsonConvert.SerializeObject(booked), Encoding.UTF8, "application/json")
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
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSuccessfullyAdded)], ToastLevel.Success);
                amountText = "";
                comment = "";
                await RefreshSaldoAsync(bookedFor);
                Payment newPayment = JsonConvert.DeserializeObject<Payment>(await response.Content.ReadAsStringAsync())!;
                newPayment.Comment = Uri.UnescapeDataString(newPayment.Comment);
                if (bookedFor == SelectedPerson) {
                    paymentsOfSelectedPerson.Add(newPayment);
                    paymentsOfSelectedPerson = paymentsOfSelectedPerson.OrderByDescending(x => x.DateTime).ToList();
                }
                base.StateHasChanged();
                if (AppData.SendConfirmationMail) {
                    if (person![person.FindIndex(x => x.Guid.Equals(bookedFor))].Email.Equals("")) {
                        ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNoEmailEntered)], ToastLevel.Info);
                    }
                    else {
                        ToastService.ShowToast("Bestätigungsmail versenden...", ToastLevel.Info);
                        Payment d = newPayment;
                        string subject = "Bestätigung einer Einzahlung | \"Essenkasse\"";
                        string body = $"Sehr geehrte(r) {person![person.FindIndex(x => x.Guid.Equals(bookedFor))].Name}\n" +
                            $"Hiermit wird Ihnen bestätigt, daß am heutigen " +
                            $"Tage {d.DateTime:d} um {d.DateTime:t} " +
                            $"eine Einzahlung in der Höhe {d.Amount} erfolgte!\n" +
                            $"Anmerkung des Eintrages: {d.Comment}" +
                            $"\n\n" +
                            $"Ihre Kassenverwaltung";
                        await JSRuntime.InvokeAsync<object>(
                            "blazorExtensions.SendLocalEmail",
                            new object[] {
                                person![person.FindIndex(x => x.Guid.Equals(bookedFor))].Email,
                                subject,
                                body
                            }
                        );
                    }
                }
                return;
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNotAdded)] + "\n" + _loc[nameof(RessourceAppStrings.toastAdminCall)], ToastLevel.Warning);
            }
        }

        private async Task LoadNextAmountOfPayments() {
            // Request last payments of a person
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Config["API-Adress"] + "payment/requestNextAmountFromPerson"),
                Headers = {
                    { "guid", SelectedPerson.ToString() },
                    { "amount", Amount.ToString() },
                    { "alreadyLoaded", paymentsOfSelectedPerson.Count.ToString()}
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
                List<Payment> getted = JsonConvert.DeserializeObject<List<Payment>>(await response.Content.ReadAsStringAsync()) ?? new List<Payment>();
                if (getted.Count == 0) {
                    ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastNothingToLoad)], ToastLevel.Info);
                }
                else {
                    getted.ForEach(x => x.Comment = Uri.UnescapeDataString(x.Comment));
                    paymentsOfSelectedPerson.AddRange(getted);
                }
            }
            else {
                ToastService.ShowToast(_loc[nameof(RessourceAppStrings.toastSomethingWrong)], ToastLevel.Warning);
            }

            base.StateHasChanged();
        }
    }
}
