namespace Modells.Test {
    
    public class PaymentTest {

        [Fact]
        public void EqualsTest() {
            DateTime dateTime = DateTime.Now;
            Money amount = new Money(10, 95);
            Guid refGuid = Guid.NewGuid();
            Payment paymentA = new Payment();
            paymentA.DateTime = dateTime;
            paymentA.Amount = amount;
            paymentA.ReferencedPerson = refGuid;

            // complete same
            Payment paymentB = new Payment(dateTime, amount, refGuid);
            Assert.True(paymentA.Equals(paymentB));

            // different date
            paymentB = new Payment(amount, refGuid);
            Assert.False(paymentA.Equals(paymentB));

            // different amount
            paymentB = new Payment(dateTime, amount, refGuid);
            paymentB.Amount = new Money(0, 5);
            Assert.False(paymentA.Equals(paymentB));

            // different referencedPerson
            paymentB.Amount = amount;
            paymentB.ReferencedPerson = new Guid();
            Assert.False(paymentA.Equals(paymentB));
        }
    }
}
