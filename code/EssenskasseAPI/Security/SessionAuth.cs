using EssenskasseAPI.Data;
using System.Data;

namespace EssenskasseAPI.Security {

    /// <summary>Angemeldeter Benutzer einer gueltigen Sitzung.</summary>
    /// <param name="UserGuid">Guid des Benutzers.</param>
    /// <param name="Role">Rolle des Benutzers (kleinere Zahl = mehr Rechte).</param>
    public sealed record SessionInfo(Guid UserGuid, int Role);

    /// <summary>Prueft das Sitzungs-Token aus dem Header <c>Authorization: Bearer {Guid}</c>
    /// gegen die Tabelle SessionToken.</summary>
    public static class SessionAuth {

        private const string ItemKey = "EssenskasseSessionInfo";
        private const string BearerPrefix = "Bearer ";

        /// <summary>Liefert die Sitzung zur Anfrage oder <c>null</c>, wenn kein Token mitgeschickt wurde,
        /// es unbekannt oder abgelaufen ist. Das Ergebnis wird je Anfrage zwischengespeichert.</summary>
        public static SessionInfo? Authenticate(HttpContext http) {
            if (http.Items[ItemKey] is SessionInfo cached) {
                return cached;
            }

            string header = http.Request.Headers.Authorization.ToString();
            if (!header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
                || !Guid.TryParse(header[BearerPrefix.Length..].Trim(), out Guid token)) {
                return null;
            }

            Db db = new Db(http.RequestServices.GetRequiredService<IConfiguration>());
            DataTable result = db.Query(
                "SELECT u.Guid AS UserGuid, u.Role AS UserRole, s.ExpaireDate AS ExpaireDate " +
                "FROM essenskasse.SessionToken s " +
                "INNER JOIN essenskasse.User u ON u.Guid = s.UserGuid " +
                "WHERE s.Guid=@token;",
                Db.P("@token", token));
            if (result.Rows.Count != 1) {
                return null;
            }

            long expire = long.Parse(result.Rows[0]["ExpaireDate"].ToString()!);
            if (DateTime.Now.Ticks >= expire) {
                return null;
            }

            SessionInfo info = new SessionInfo(
                new Guid(result.Rows[0]["UserGuid"].ToString()!),
                int.Parse(result.Rows[0]["UserRole"].ToString()!));
            http.Items[ItemKey] = info;
            return info;
        }

        /// <summary>Ist der Benutzer Administrator? Das gilt fuer Rollen bis einschliesslich
        /// <c>Security:AdminMaxRole</c> (Standard 100, also jede Rolle; zum Einschraenken z. B. auf 10 setzen).</summary>
        public static bool IsAdmin(HttpContext http, SessionInfo session) {
            int adminMaxRole = http.RequestServices.GetRequiredService<IConfiguration>()
                .GetValue<int>("Security:AdminMaxRole", 100);
            return session.Role <= adminMaxRole;
        }
    }
}
