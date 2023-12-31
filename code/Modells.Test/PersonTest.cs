namespace Modells.Test {

    public class PersonTest {

        [Fact]
        public void EqualsTest() {
            Guid guid = Guid.NewGuid();
            string name = "A";
            string email = "test";
            Money money = new Money(154, 34);
            Person personA = new Person(guid, name, email, money);

            // different Guid
            Person personB = new Person(name, email, money);
            Assert.False(personA.Equals(personB));

            // complete same
            personB.Guid = guid;
            Assert.True(personA.Equals(personB));

            // different name
            Person personC = new Person(guid, "Heinz", email);
            Assert.False(personA.Equals(personC));

            // different email
            Person personD = new Person("name", "tset");
            personD.Guid = guid;
            Assert.False(personA.Equals(personD));

            // different saldo
            Person personE = new Person(guid, name, email, new Money(4231, 23));
            Assert.False(personA.Equals(personE));
        }

        [Fact]
        public void ConstructorTest() {
            Guid guid = Guid.NewGuid();
            string name = "A";
            string email = "test";
            Money money = new Money(154, 34);
            Person personA = new Person(guid, name, email, money);

            Person personT = new Person();
            Assert.NotEqual(personA.Guid, personT.Guid);

            personT = new Person(guid, name, null, money);
            Assert.Equal("", personT.Email);

            personT = new Person(guid, name, null);
            Assert.Equal("", personT.Email);

            personT = new Person(name: null);
            Assert.Equal("", personT.Name);

            personT = new Person(name, null, money);
            Assert.Equal("", personT.Email);

            personT = new Person(name: null, email: null);
            Assert.Equal("", personT.Name);
            Assert.Equal("", personT.Email);
        }
    }
}
