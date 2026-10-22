namespace Modells.Test {

    public class EntityChangesTest {

        private static readonly Guid PersonA = Guid.NewGuid();
        private static readonly Guid PersonB = Guid.NewGuid();
        private static readonly Provider ProviderA = new(Guid.NewGuid(), "A");
        private static readonly Provider ProviderB = new(Guid.NewGuid(), "B");

        private static Payment NewPayment(string? comment = "Kasse") =>
            new(new DateTime(2026, 5, 1, 12, 0, 0), new Money(10, 50), PersonA, comment!);

        [Theory]
        [InlineData(1250, "12,50")]
        [InlineData(5, "0,05")]
        [InlineData(0, "0,00")]
        [InlineData(-305, "-3,05")]
        [InlineData(100000, "1000,00")]
        public void ToInputText_FormatsWithComma(long cents, string expected) {
            Assert.Equal(expected, EntityChanges.ToInputText(Money.FromTotalCents(cents)));
        }

        [Theory]
        [InlineData(1250)]
        [InlineData(5)]
        [InlineData(-305)]
        public void ToInputText_CanBeParsedAgain(long cents) {
            Assert.True(MoneyParser.TryParse(EntityChanges.ToInputText(Money.FromTotalCents(cents)), out Money parsed));
            Assert.Equal(cents, parsed.ToTotalCents());
        }

        [Fact]
        public void PaymentChanged_NothingChanged_IsFalse() {
            Assert.False(EntityChanges.PaymentChanged(NewPayment(), new Money(10, 50), "Kasse", PersonA));
        }

        [Fact]
        public void PaymentChanged_IgnoresWhitespaceAroundComment() {
            Assert.False(EntityChanges.PaymentChanged(NewPayment(), new Money(10, 50), "  Kasse ", PersonA));
        }

        [Fact]
        public void PaymentChanged_NullCommentEqualsEmpty() {
            Assert.False(EntityChanges.PaymentChanged(NewPayment(null), new Money(10, 50), "", PersonA));
        }

        [Fact]
        public void PaymentChanged_DetectsEachField() {
            Assert.True(EntityChanges.PaymentChanged(NewPayment(), new Money(10, 51), "Kasse", PersonA));
            Assert.True(EntityChanges.PaymentChanged(NewPayment(), new Money(10, 50), "Bar", PersonA));
            Assert.True(EntityChanges.PaymentChanged(NewPayment(), new Money(10, 50), "Kasse", PersonB));
        }

        [Fact]
        public void OrderChanged_DetectsEachField() {
            Order order = new(new DateTime(2026, 5, 1), new Money(7, 0), ProviderA, PersonA);
            Assert.False(EntityChanges.OrderChanged(order, new Money(7, 0), ProviderA.Guid, PersonA));
            Assert.True(EntityChanges.OrderChanged(order, new Money(7, 5), ProviderA.Guid, PersonA));
            Assert.True(EntityChanges.OrderChanged(order, new Money(7, 0), ProviderB.Guid, PersonA));
            Assert.True(EntityChanges.OrderChanged(order, new Money(7, 0), ProviderA.Guid, PersonB));
        }
    }
}
