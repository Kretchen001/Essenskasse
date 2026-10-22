using System.Security.Cryptography;

namespace EssenskasseAPI.Security {

    /// <summary>Speichert Passwoerter als PBKDF2-HMAC-SHA256-Hash mit zufaelligem Salt.
    /// Format: <c>pbkdf2-sha256$Iterationen$Salt(Base64)$Hash(Base64)</c> (passt in VARCHAR(255)).
    /// Alte Klartext-Passwoerter werden noch erkannt (<see cref="NeedsRehash"/>), damit sie beim
    /// naechsten erfolgreichen Login automatisch durch einen Hash ersetzt werden koennen.</summary>
    public static class PasswordHasher {

        private const string Prefix = "pbkdf2-sha256";
        private const int Iterations = 600_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        /// <summary>Erzeugt den Hash eines Passworts mit neuem Salt.</summary>
        public static string Hash(string password) {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>Ist der gespeicherte Wert ein Hash (und kein altes Klartext-Passwort)?</summary>
        public static bool IsHashed(string stored) {
            return stored.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        /// <summary>Muss der gespeicherte Wert neu gehasht werden (Klartext oder alte Iterationszahl)?</summary>
        public static bool NeedsRehash(string stored) {
            if (!TryParse(stored, out int iterations, out _, out _)) {
                return true;
            }
            return iterations < Iterations;
        }

        /// <summary>Prueft ein Passwort gegen den gespeicherten Wert (Hash oder altes Klartext-Passwort)
        /// in konstanter Zeit.</summary>
        public static bool Verify(string password, string stored) {
            if (!IsHashed(stored)) {
                // Altbestand: Klartext vergleichen
                return CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(password),
                    System.Text.Encoding.UTF8.GetBytes(stored));
            }
            if (!TryParse(stored, out int iterations, out byte[] salt, out byte[] expected)) {
                return false;
            }
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        private static bool TryParse(string stored, out int iterations, out byte[] salt, out byte[] hash) {
            iterations = 0;
            salt = [];
            hash = [];
            string[] parts = stored.Split('$');
            if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out iterations) || iterations <= 0) {
                return false;
            }
            try {
                salt = Convert.FromBase64String(parts[2]);
                hash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException) {
                return false;
            }
            return salt.Length > 0 && hash.Length > 0;
        }
    }
}
