using Microsoft.AspNetCore.Components;
using Modells;
using Modells.Statistics;
using System.Globalization;

namespace Essenskasse.Cards {

    /// <summary>Eine Reihe von Saeulen (z. B. Einzahlungen) mit einem Wert je Beschriftung.</summary>
    /// <param name="CssClass">ek-series-0 bis ek-series-3: legt die Farbe fest.</param>
    public sealed record BarSeries(string Name, IReadOnlyList<long> Values, string CssClass);

    /// <summary>Gruppiertes Saeulendiagramm (SVG). Die Werte sind ganze Zahlen (z. B. Cent); <see cref="Format"/> macht daraus Text.</summary>
    public partial class BarChart {

        /// <summary>Beschriftung je Gruppe (z. B. Monat).</summary>
        [Parameter]
        public IReadOnlyList<string> Labels { get; set; } = [];

        /// <summary>Die Reihen; jede hat so viele Werte wie Beschriftungen.</summary>
        [Parameter]
        public IReadOnlyList<BarSeries> Series { get; set; } = [];

        /// <summary>Beschreibung fuer Screenreader.</summary>
        [Parameter]
        public string Title { get; set; } = "";

        /// <summary>Wandelt einen Wert in Text um; Standard ist ein Geldbetrag in Cent.</summary>
        [Parameter]
        public Func<long, string>? Format { get; set; }

        private const double Width = 720;
        private const double Height = 300;
        private const double Left = 92;
        private const double Right = 14;
        private const double Top = 14;
        private const double Bottom = 34;

        private List<long> YTicks = [];
        private long YLow;
        private long YHigh = 1;

        protected override void OnParametersSet() {
            List<long> all = Series.SelectMany(x => x.Values).ToList();
            if (all.Count == 0) {
                YTicks = [];
                return;
            }
            YTicks = ChartScale.NiceTicks(Math.Min(0, all.Min()), Math.Max(0, all.Max()));
            YLow = YTicks[0];
            YHigh = YTicks[^1];
        }

        private double GroupWidth => (Width - Left - Right) / Math.Max(1, Labels.Count);

        private double BarWidth => Math.Min(40, GroupWidth * 0.8 / Math.Max(1, Series.Count));

        /// <summary>Zeigt bei vielen Gruppen nur jede n-te Beschriftung, damit sie sich nicht ueberlagern.</summary>
        private int LabelStep => Math.Max(1, (int)Math.Ceiling(Labels.Count / 12.0));

        private double GroupCenter(int group) => Left + GroupWidth * (group + 0.5);

        private double BarX(int group, int series) => GroupCenter(group) - BarWidth * Series.Count / 2 + BarWidth * series;

        private double Y(long value) => Top + (Height - Top - Bottom) * (YHigh - value) / Math.Max(1, YHigh - YLow);

        private static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        private string FormatValue(long value) => (Format ?? (v => Money.FromTotalCents(v).ToString()))(value);
    }
}
