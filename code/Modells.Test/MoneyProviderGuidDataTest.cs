namespace Modells.Test {

    public class SessionTokenTest {

        [Fact]
        public void CtorTestEmpy() {
            MoneyProviderGuidData m = new MoneyProviderGuidData();
            Assert.Null(m.Money);
            Assert.Null(m.Provider);
            Assert.Null(m.ReferencedPerson);
        }
    }
}