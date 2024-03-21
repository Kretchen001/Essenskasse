namespace Modells.Test {

    public class TransactionProcedure {

        [Fact]
        public void ConstructorTest() {
            Modells.TransactionProcedure tp = new Modells.TransactionProcedure() {
                DateTime = DateTime.Now,
                Amount = new Money(),
                Comment = ""
            };
        }
    }
}
