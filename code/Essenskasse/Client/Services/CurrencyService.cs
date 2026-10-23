using Modells;

namespace Essenskasse.Client.Services {

    /// <summary>Haelt die Waehrung der Anzeige. Sie steht in <see cref="Money.CurrentCurrency"/>, damit jede Betragsanzeige
    /// ohne weitere Aenderung dieselbe Waehrung benutzt. Nach einem Wechsel baut sich die Oberflaeche neu auf (MainLayout hoert auf <see cref="Changed"/>).</summary>
    public sealed class CurrencyService {

        /// <summary>Wird nach jedem Waehrungswechsel ausgeloest.</summary>
        public event Action? Changed;

        /// <summary>Zaehlt die Waehrungswechsel; zusammen mit der Sprache der <c>@key</c> der Oberflaeche.</summary>
        public int Version { get; private set; }

        /// <summary>Stellt die Waehrung um (nur die Anzeige, es wird nichts umgerechnet).</summary>
        public void Set(Currency currency) {
            if (Money.CurrentCurrency == currency) {
                return;
            }
            Money.CurrentCurrency = currency;
            Version++;
            Changed?.Invoke();
        }
    }
}
