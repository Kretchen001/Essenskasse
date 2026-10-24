namespace Modells.Statistics {

    /// <summary>Wie lange und wie oft eine Person im Beobachtungszeitraum "im Minus" war.</summary>
    /// <param name="InMinus">Gesamtdauer unter oder auf dem Grenzwert.</param>
    /// <param name="Observed">Beobachtete Gesamtdauer (Nenner der Quote).</param>
    /// <param name="Phases">Anzahl getrennter Minus-Phasen (auch eine, die schon vor Beginn lief).</param>
    /// <param name="Longest">Laenge der laengsten Phase (im Zeitraum).</param>
    /// <param name="EndBalance">Saldo am Ende des Zeitraums (Cent).</param>
    public sealed record PersonMinus(Person Person, TimeSpan InMinus, TimeSpan Observed, int Phases, TimeSpan Longest, long EndBalance, bool InMinusAtEnd) {

        /// <summary>Anteil der Zeit im Minus von 0 bis 1.</summary>
        public double Share => Observed <= TimeSpan.Zero ? 0 : InMinus / Observed;
    }

    /// <summary>"Im Minus" heisst: Saldo ist nicht groesser als der Grenzwert (wie im Dashboard: gut ist, was ueber dem Grenzwert liegt).</summary>
    public static class MinusAnalysis {

        /// <summary>Wertet jede Person aus. Der Anfangssaldo wird aus dem heutigen Saldo und allen Buchungen zurueckgerechnet.
        /// Beobachtet wird vom Beginn des Tags <paramref name="from"/> bis zum Ende des Tags <paramref name="to"/>, hoechstens bis <paramref name="asOf"/>.</summary>
        /// <param name="limitCents">Grenzwert in Cent.</param>
        public static List<PersonMinus> Analyze(StatisticsData data, DateTime from, DateTime to, long limitCents, DateTime asOf) {
            DateTime start = from.Date;
            DateTime end = to.Date.AddDays(1);
            if (end > asOf) {
                end = asOf;
            }
            List<PersonMinus> result = [];
            if (end <= start) {
                return result;
            }
            ILookup<Guid, BalanceEvent> byPerson = data.Events.ToLookup(x => x.Person);
            foreach (Person person in data.Persons) {
                List<BalanceEvent> events = byPerson[person.Guid].OrderBy(x => x.When).ToList();
                long balance = person.Saldo.ToTotalCents() - events.Sum(x => x.Cents);
                int index = 0;
                while (index < events.Count && events[index].When < start) {
                    balance += events[index].Cents;
                    index++;
                }
                TimeSpan inMinus = TimeSpan.Zero;
                TimeSpan longest = TimeSpan.Zero;
                TimeSpan running = TimeSpan.Zero;
                int phases = 0;
                bool minus = balance <= limitCents;
                if (minus) {
                    phases = 1;
                }
                DateTime cursor = start;
                void CloseSegment(DateTime until) {
                    if (minus) {
                        TimeSpan length = until - cursor;
                        inMinus += length;
                        running += length;
                        if (running > longest) {
                            longest = running;
                        }
                    }
                    cursor = until;
                }
                for (; index < events.Count && events[index].When < end; index++) {
                    CloseSegment(events[index].When);
                    balance += events[index].Cents;
                    bool nowMinus = balance <= limitCents;
                    if (nowMinus && !minus) {
                        phases++;
                    }
                    if (!nowMinus) {
                        running = TimeSpan.Zero;
                    }
                    minus = nowMinus;
                }
                CloseSegment(end);
                result.Add(new PersonMinus(person, inMinus, end - start, phases, longest, balance, minus));
            }
            return result;
        }
    }
}
