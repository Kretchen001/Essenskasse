namespace Modells.Test {

    public class OrderTest {

        [Fact]
        public void EqualsTest() {
            DateTime dateTime = DateTime.Now;
            Provider p = new Provider("Hallo");
            Guid guid = Guid.NewGuid();
            Order orderA = new Order(
                dateTime: dateTime,
                money: new Money(1,23),
                provider: p,
                referencedPerson: guid
            );
            Order orderB = new Order {
                DateTime = orderA.DateTime,
                Money = orderA.Money,
                Provider = orderA.Provider,
                ReferencedPerson = orderA.ReferencedPerson
            };

            Assert.True(orderA.Equals(orderB));
            orderB.DateTime = dateTime;
            Assert.True(orderA.Equals(orderB));
            orderB.ReferencedPerson = new Guid();
            Assert.False(orderA.Equals(orderB));
            orderB.Provider = new Provider("Hello");
            Assert.False(orderA.Equals(orderB));

            orderB = new Order(orderA.Money, orderA.Provider, orderA.ReferencedPerson);
            Assert.False(orderA.Equals(orderB));
            Assert.NotEqual(orderA.DateTime, orderB.DateTime);
        }
    }
}
