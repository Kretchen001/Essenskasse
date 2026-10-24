using Microsoft.AspNetCore.Components;
using Modells;
using Modells.Statistics;
using System.Globalization;
using System.Text;

namespace Essenskasse.Cards {

    /// <summary>Liniendiagramm (SVG) fuer einen Verlauf von Betraegen ueber die Tage. Farben kommen aus dem Theme.</summary>
    public partial class LineChart {

        /// <summary>Die Punkte, nach Tag aufsteigend.</summary>
        [Parameter]
        public IReadOnlyList<DayValue> Points { get; set; } = [];

        /// <summary>Beschreibung fuer Screenreader.</summary>
        [Parameter]
        public string Title { get; set; } = "";

        /// <summary>Optionale waagerechte Hilfslinie (Cent), z. B. ein Grenzwert.</summary>
        [Parameter]
        public long? ReferenceLine { get; set; }

        private const double Width = 720;
        private const double Height = 280;
        private const double Left = 92;
        private const double Right = 14;
        private const double Top = 14;
        private const double Bottom = 30;

        private List<long> YTicks = [];
        private long YLow;
        private long YHigh = 1;

        protected override void OnParametersSet() {
            if (Points.Count == 0) {
                YTicks = [];
                return;
            }
            long min = Points.Min(x => x.Cents);
            long max = Points.Max(x => x.Cents);
            if (ReferenceLine is not null) {
                min = Math.Min(min, ReferenceLine.Value);
                max = Math.Max(max, ReferenceLine.Value);
            }
            YTicks = ChartScale.NiceTicks(min, max);
            YLow = YTicks[0];
            YHigh = YTicks[^1];
        }

        private double HitWidth => Points.Count < 2 ? 0 : (Width - Left - Right) / (Points.Count - 1);

        private double X(int index) => Left + (Width - Left - Right) * index / Math.Max(1, Points.Count - 1);

        private double Y(long cents) => Top + (Height - Top - Bottom) * (YHigh - cents) / Math.Max(1, YHigh - YLow);

        private static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

        private static string FormatValue(long cents) => Money.FromTotalCents(cents).ToString();

        private string LinePoints {
            get {
                StringBuilder text = new();
                for (int i = 0; i < Points.Count; i++) {
                    text.Append(F(X(i))).Append(',').Append(F(Y(Points[i].Cents))).Append(' ');
                }
                return text.ToString().TrimEnd();
            }
        }

        /// <summary>Flaeche zwischen Linie und unterem Rand (bzw. Nulllinie, wenn sie im Bild liegt).</summary>
        private string AreaPath {
            get {
                double baseline = Y(YLow < 0 && YHigh > 0 ? 0 : YLow);
                StringBuilder path = new();
                path.Append('M').Append(F(X(0))).Append(',').Append(F(baseline));
                for (int i = 0; i < Points.Count; i++) {
                    path.Append(" L").Append(F(X(i))).Append(',').Append(F(Y(Points[i].Cents)));
                }
                path.Append(" L").Append(F(X(Points.Count - 1))).Append(',').Append(F(baseline)).Append(" Z");
                return path.ToString();
            }
        }

        /// <summary>Etwa fuenf Datumsbeschriftungen entlang der Zeitachse.</summary>
        private IEnumerable<(int Index, string Label)> XLabels {
            get {
                int last = Points.Count - 1;
                int count = Math.Min(5, Points.Count);
                return Enumerable.Range(0, count)
                    .Select(i => (int)Math.Round(i * last / (double)Math.Max(1, count - 1)))
                    .Distinct()
                    .Select(index => (index, Points[index].Day.ToString("d")));
            }
        }
    }
}
