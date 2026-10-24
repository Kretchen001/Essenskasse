using Newtonsoft.Json;

namespace Modells.Test {

    public class ReversalTest {

        [Fact]
        public void NewBookings_AreNotReversed() {
            Payment payment = new(new Money(5, 0), Guid.NewGuid(), "x");
            Order order = new(new Money(5, 0), new Provider(), Guid.NewGuid());
            Assert.Null(payment.ReversalOf);
            Assert.Null(payment.ReversedBy);
            Assert.Null(order.ReversalOf);
            Assert.Null(order.ReversedBy);
        }

        [Fact]
        public void Payment_KeepsReversalLinksThroughJson() {
            Payment payment = new(new DateTime(2026, 10, 23), new Money(-5, 0), Guid.NewGuid(), "x") { ReversalOf = 123, ReversedBy = 456 };
            Payment back = JsonConvert.DeserializeObject<Payment>(JsonConvert.SerializeObject(payment))!;
            Assert.Equal(123, back.ReversalOf);
            Assert.Equal(456, back.ReversedBy);
        }

        [Fact]
        public void Order_KeepsReversalLinksThroughJson() {
            Order order = new(new DateTime(2026, 10, 23), new Money(-5, 0), new Provider(), Guid.NewGuid()) { ReversalOf = 123 };
            Order back = JsonConvert.DeserializeObject<Order>(JsonConvert.SerializeObject(order))!;
            Assert.Equal(123, back.ReversalOf);
            Assert.Null(back.ReversedBy);
        }
    }
}
