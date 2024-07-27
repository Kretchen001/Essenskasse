using U = Modells.User;

namespace Modells.Test.User {

    public class UserTest {

        [Fact]
        public void CtorTestEmpy() {
            U.User user = new U.User();
            Assert.Equal(user.Guid, new Guid());
            Assert.Equal(user.Name, string.Empty);
            Assert.Equal(user.UserName, string.Empty);
            Assert.Equal(user.Pwd, string.Empty);
            Assert.Equal(user.Email, string.Empty);
            Assert.Equal(0, user.Role);
        }

        [Fact]
        public void CtorTestJsonConstructor() {
            U.User user = new U.User("", "", "", "", 1);
        }

        [Fact]
        public void CtorTestNormal() {
            U.User user = new U.User(new Guid(), "", "", "", "", 1);
        }
    }
}