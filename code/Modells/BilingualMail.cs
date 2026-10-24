using System.Text;

namespace Modells {

    /// <summary>Ein Teil einer Mail in einer Sprache.</summary>
    /// <param name="Language">Anzeigename der Sprache, z. B. "Deutsch".</param>
    public sealed record MailPart(string Language, string Subject, string Body);

    /// <summary>Fuegt dieselbe Mail in mehreren Sprachen zu einer Mail zusammen: ein Betreff mit allen Betreffzeilen,
    /// im Text jede Sprache unter einer Trennlinie mit dem Namen der Sprache. Bei nur einer Sprache bleibt die Mail unveraendert.</summary>
    public static class BilingualMail {

        /// <summary>Trennlinie zwischen den Sprachen.</summary>
        public const string Divider = "────────────────────────────";

        /// <summary>Trennzeichen zwischen den Betreffzeilen.</summary>
        public const string SubjectSeparator = " / ";

        /// <summary>Betreff und Text der zusammengefuegten Mail (Reihenfolge wie in <paramref name="parts"/>).</summary>
        public static (string Subject, string Body) Combine(IReadOnlyList<MailPart> parts) {
            if (parts.Count == 0) {
                return ("", "");
            }
            if (parts.Count == 1) {
                return (parts[0].Subject, parts[0].Body);
            }
            string subject = string.Join(SubjectSeparator, parts.Select(x => x.Subject).Distinct());
            StringBuilder body = new();
            for (int i = 0; i < parts.Count; i++) {
                if (i > 0) {
                    body.Append("\n\n");
                }
                body.Append(Divider).Append('\n').Append(parts[i].Language).Append('\n').Append(Divider).Append("\n\n").Append(parts[i].Body);
            }
            return (subject, body.ToString());
        }
    }
}
