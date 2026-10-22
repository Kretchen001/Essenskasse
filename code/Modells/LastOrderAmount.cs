using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Der Betrag der letzten Bestellung einer Person (Vorschlag beim Bestellen).</summary>
    public class LastOrderAmount {

        /// <summary>Die Person, zu der die Bestellung gehoert.</summary>
        [JsonInclude]
        public Guid ReferencedPerson { get; set; } = Guid.Empty;

        /// <summary>Der Betrag der letzten Bestellung.</summary>
        [JsonInclude]
        public Money Money { get; set; } = new Money();

        /// <summary>Zeitpunkt der letzten Bestellung.</summary>
        [JsonInclude]
        public DateTime DateTime { get; set; } = new DateTime();

        /// <summary>Leerer Konstruktor fuer die Serialisierung.</summary>
        public LastOrderAmount() { }
    }
}
