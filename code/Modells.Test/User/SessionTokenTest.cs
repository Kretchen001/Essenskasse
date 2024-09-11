using U = Modells.User;

namespace Modells.Test.User {

    public class SessionTokenTest {

        [Fact]
        public void CtorTestEmpy() {
            U.SessionToken token = new U.SessionToken();
            Assert.Equal(token.Guid, new Guid());
            Assert.Equal(token.UserGuid, new Guid());
            Assert.Equal(0, token.ExpaireDate);
        }

        [Fact]
        public void CtorTestJsonConstructor() {
            U.SessionToken token = new U.SessionToken(new Guid(), new Guid(), 0);
        }
    }
}