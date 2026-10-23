using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Essenskasse.Client.Localization {

    /// <summary>Liefert die Texte aus dem <see cref="TextService"/> in der gerade gueltigen Sprache.
    /// Ersetzt die Resx-Satellitendateien, die bei Blazor WebAssembly nur fuer die Startsprache geladen werden.</summary>
    public sealed class JsonStringLocalizer<T> : IStringLocalizer<T> {

        private readonly TextService _texts;

        public JsonStringLocalizer(TextService texts) {
            _texts = texts;
        }

        public LocalizedString this[string name] =>
            _texts.TryGet(name, out string value) ? new LocalizedString(name, value, false) : new LocalizedString(name, name, true);

        public LocalizedString this[string name, params object[] arguments] =>
            _texts.TryGet(name, out string value)
                ? new LocalizedString(name, string.Format(CultureInfo.CurrentCulture, value, arguments), false)
                : new LocalizedString(name, name, true);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _texts.GetAll().Select(x => new LocalizedString(x.Key, x.Value, false));
    }
}
