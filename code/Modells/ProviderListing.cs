namespace Modells {

    /// <summary>Sortierung der Anbieterliste.</summary>
    public enum ProviderSort {
        NameAsc,
        NameDesc,
        LastOrderedFirst,
        MostOrdersFirst
    }

    /// <summary>Filtert und sortiert die Anbieterliste (ohne Beachtung der Schreibweise).</summary>
    public static class ProviderListing {

        /// <summary>
        /// Liefert die Anbieter, deren Bezeichnung den Suchtext enthaelt, in der gewuenschten Reihenfolge.
        /// Anbieter ohne Auswertung gelten als nie bestellt und stehen bei den Bestell-Sortierungen hinten.
        /// </summary>
        public static List<Provider> Apply(IEnumerable<Provider> providers, string? filter, ProviderSort sort,
                                           IReadOnlyDictionary<Guid, ProviderStatistics>? statistics = null) {
            string text = (filter ?? "").Trim();
            IEnumerable<Provider> result = providers;
            if (text.Length > 0) {
                result = result.Where(x => x.Designation.Contains(text, StringComparison.CurrentCultureIgnoreCase));
            }

            StringComparer name = StringComparer.CurrentCultureIgnoreCase;
            DateTime LastOf(Provider x) => statistics is not null && statistics.TryGetValue(x.Guid, out ProviderStatistics? s)
                ? s.LastOrder ?? DateTime.MinValue : DateTime.MinValue;
            int CountOf(Provider x) => statistics is not null && statistics.TryGetValue(x.Guid, out ProviderStatistics? s)
                ? s.OrderCount : 0;

            return sort switch {
                ProviderSort.NameDesc => result.OrderByDescending(x => x.Designation, name).ToList(),
                ProviderSort.LastOrderedFirst => result.OrderByDescending(LastOf).ThenBy(x => x.Designation, name).ToList(),
                ProviderSort.MostOrdersFirst => result.OrderByDescending(CountOf).ThenBy(x => x.Designation, name).ToList(),
                _ => result.OrderBy(x => x.Designation, name).ToList(),
            };
        }
    }
}
