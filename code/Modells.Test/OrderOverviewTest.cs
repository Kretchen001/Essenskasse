namespace Modells.Test {

    public class OrderOverviewTest {

        private static readonly Guid A = Guid.NewGuid();
        private static readonly Guid B = Guid.NewGuid();

        private static Order O(DateTime when, Guid person, long euro, short cent) =>
            new(when, new Money(euro, cent), new Provider(), person);

        [Fact]
        public void Empty_GivesNoDays() {
            Assert.Empty(OrderOverview.Build([]));
        }

        [Fact]
        public void Days_AreNewestFirst_AndCentsAreSummedPerPerson() {
            DateTime d1 = new(2026, 10, 20, 0, 0, 0);
            DateTime d2 = new(2026, 10, 21, 0, 0, 0);
            List<OrderOverviewDay> days = OrderOverview.Build([
                O(d1, A, 4, 50),
                O(d2.AddMilliseconds(1), A, 3, 0),
                O(d2.AddMilliseconds(2), A, 2, 5),
                O(d2.AddMilliseconds(3), B, 6, 0),
            ]);
            Assert.Equal(2, days.Count);
            Assert.Equal(d2, days[0].Date);
            Assert.Equal(505, days[0].CentsByPerson[A]);
            Assert.Equal(600, days[0].CentsByPerson[B]);
            Assert.Equal(1105, days[0].TotalCents);
            Assert.Equal(450, days[1].TotalCents);
            Assert.False(days[1].CentsByPerson.ContainsKey(B));
        }

        [Fact]
        public void OrdersOfOneDayAcrossLoadSteps_AreMerged() {
            DateTime day = new(2026, 10, 21, 0, 0, 0);
            List<Order> firstLoad = [O(day.AddMilliseconds(1), A, 1, 0)];
            List<Order> secondLoad = [O(day.AddMilliseconds(2), B, 2, 0)];
            List<OrderOverviewDay> days = OrderOverview.Build(firstLoad.Concat(secondLoad));
            Assert.Single(days);
            Assert.Equal(300, days[0].TotalCents);
        }

        [Fact]
        public void PeopleWithOrders_ListsEachPersonOnce() {
            DateTime day = new(2026, 10, 21);
            HashSet<Guid> people = OrderOverview.PeopleWithOrders(OrderOverview.Build([O(day, A, 1, 0), O(day.AddDays(-1), A, 1, 0)]));
            Assert.Single(people);
            Assert.Contains(A, people);
        }
    }
}
