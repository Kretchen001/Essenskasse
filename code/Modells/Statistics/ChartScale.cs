namespace Modells.Statistics {

    /// <summary>Hilfen fuer die Achsen der Diagramme.</summary>
    public static class ChartScale {

        /// <summary>"Runde" Werte fuer eine Achse: aufsteigend, der erste nicht ueber <paramref name="min"/>, der letzte nicht unter
        /// <paramref name="max"/>, mit Schritten 1, 2, 2,5 oder 5 mal Zehnerpotenz. <paramref name="target"/> ist die ungefaehre Zahl der Teilstrecken.</summary>
        public static List<long> NiceTicks(long min, long max, int target = 4) {
            if (max < min) {
                (min, max) = (max, min);
            }
            if (min == max) {
                long pad = Math.Max(100, Math.Abs(min) / 10);
                min -= pad;
                max += pad;
            }
            double raw = (max - min) / (double)Math.Max(1, target);
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            double step = magnitude;
            foreach (double factor in new[] { 1.0, 2.0, 2.5, 5.0, 10.0 }) {
                step = factor * magnitude;
                if (step >= raw) {
                    break;
                }
            }
            long stepLong = Math.Max(1, (long)Math.Round(step));
            long first = (long)Math.Floor(min / (double)stepLong) * stepLong;
            List<long> ticks = [];
            for (long value = first; ; value += stepLong) {
                ticks.Add(value);
                if (value >= max) {
                    break;
                }
            }
            return ticks;
        }
    }
}
