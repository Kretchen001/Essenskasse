using Newtonsoft.Json;

namespace Modells {

    /// <summary>Auswertung der Bestellungen eines Anbieters (Anzahl, Summe, letzte Bestellung).</summary>
    public class ProviderStatistics {

        /// <summary>Anbieter, auf den sich die Auswertung bezieht.</summary>
        public Guid ProviderGuid { get; set; } = new Guid();
        /// <summary>Anzahl aller Bestellungen bei diesem Anbieter.</summary>
        public int OrderCount { get; set; } = 0;
        /// <summary>Summe aller Bestellungen in Cent.</summary>
        public long TotalCents { get; set; } = 0;
        /// <summary>Zeitpunkt der letzten Bestellung; null, wenn noch nie bestellt wurde.</summary>
        public DateTime? LastOrder { get; set; } = null;

        /// <summary>Summe aller Bestellungen als Geldbetrag.</summary>
        [JsonIgnore]
        public Money Total => Money.FromTotalCents(TotalCents);

        /// <summary></summary>
        public ProviderStatistics() { }

        /// <summary></summary>
        public ProviderStatistics(Guid providerGuid, int orderCount, long totalCents, DateTime? lastOrder) {
            ProviderGuid = providerGuid;
            OrderCount = orderCount;
            TotalCents = totalCents;
            LastOrder = lastOrder;
        }
    }
}
