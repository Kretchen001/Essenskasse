using EssenskasseAPI.Security;

namespace EssenskasseAPI.Test {

    public class PasswordHasherTest {

        [Fact]
        public void HashAndVerify_CorrectPassword() {
            string hash = PasswordHasher.Hash("geheim");
            Assert.True(PasswordHasher.IsHashed(hash));
            Assert.True(PasswordHasher.Verify("geheim", hash));
        }

        [Fact]
        public void Verify_WrongPassword() {
            string hash = PasswordHasher.Hash("geheim");
            Assert.False(PasswordHasher.Verify("Geheim", hash));
            Assert.False(PasswordHasher.Verify("", hash));
        }

        [Fact]
        public void Hash_UsesRandomSalt() {
            Assert.NotEqual(PasswordHasher.Hash("geheim"), PasswordHasher.Hash("geheim"));
        }

        [Fact]
        public void Hash_FitsIntoVarchar255() {
            Assert.True(PasswordHasher.Hash(new string('x', 500)).Length <= 255);
        }

        [Fact]
        public void LegacyPlaintext_IsVerifiedAndMarkedForRehash() {
            Assert.False(PasswordHasher.IsHashed("altesPasswort"));
            Assert.True(PasswordHasher.Verify("altesPasswort", "altesPasswort"));
            Assert.False(PasswordHasher.Verify("anderes", "altesPasswort"));
            Assert.True(PasswordHasher.NeedsRehash("altesPasswort"));
        }

        [Fact]
        public void FreshHash_NeedsNoRehash() {
            Assert.False(PasswordHasher.NeedsRehash(PasswordHasher.Hash("geheim")));
        }

        [Fact]
        public void BrokenHash_NeverVerifies() {
            Assert.False(PasswordHasher.Verify("x", "pbkdf2-sha256$600000$%%%$%%%"));
            Assert.False(PasswordHasher.Verify("x", "pbkdf2-sha256$abc$AAAA$AAAA"));
        }
    }
}
