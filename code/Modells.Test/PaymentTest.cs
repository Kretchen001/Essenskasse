namespace Modells.Test {
    
    public class PaymentTest {

        [Fact]
        public void EqualsTest() {
            DateTime dateTime = DateTime.Now;
            Money amount = new Money(10, 95);
            Guid refGuid = Guid.NewGuid();
            Payment paymentA = new Payment() {
                DateTime = dateTime,
                Amount = amount,
                ReferencedPerson = refGuid,
                Comment = "lorem ipsum"
            };

            // complete same
            Payment paymentB = new Payment(dateTime, amount, refGuid, "lorem ipsum");
            Assert.True(paymentA.Equals(paymentB));

            // different comment
            paymentB = new Payment(dateTime, amount, refGuid, "yo");
            Assert.False(paymentB.Equals(paymentA));

            // different referencedPerson
            paymentB = new Payment(dateTime, amount, Guid.NewGuid());
            Assert.False(paymentB.Equals(paymentA));

            // different amount
            paymentB = new Payment(new Money(95, 10), refGuid);
            Assert.False(paymentB.Equals(paymentA));

            // different date
            paymentB = new Payment(new Money(95, 10), refGuid, "com"); // datetime is DateTime.Now and so different
            Assert.False(paymentB.Equals(paymentA));
        }
    }
}
