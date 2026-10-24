namespace Essenskasse.Client {

    /// <summary>Kleine Hilfen fuer die Darstellung von Personen.</summary>
    public static class PersonText {

        /// <summary>Anfangsbuchstaben fuer den Kreis neben dem Namen (Vor- und Nachname, sonst nur der erste).</summary>
        public static string Initials(string? name) {
            string[] parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) {
                return "?";
            }
            if (parts.Length == 1) {
                return char.ToUpper(parts[0][0]).ToString();
            }
            return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}";
        }
    }
}
