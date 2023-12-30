namespace Modells.Test {

    public class ProviderTest {

        [Fact]
        public void EqualsTest() {
            Guid guid = Guid.NewGuid();
            Provider providerA = new Provider(guid, "Designation");
            Provider providerB = new Provider();
            Assert.False(providerA.Equals(providerB));

            Provider providerC = new Provider("Designation");
            Assert.False(providerA.Equals(providerC));
            providerC.Designation = "No?";
            Assert.False(providerA.Equals(providerC));

            providerB.Guid = guid;
            providerB.Designation = "Designation";
            Assert.True(providerA.Equals(providerB));
            providerB.Designation = "No!";
            Assert.False(providerA.Equals(providerB));
        }
    }
}
