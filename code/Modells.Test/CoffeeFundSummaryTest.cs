// Im Testprojekt gibt es bereits eine Klasse Modells.Test.TransactionProcedure; deshalb der Alias auf den Typ des Modells.
using Tp = global::Modells.TransactionProcedure;

namespace Modells.Test {

    public class CoffeeFundSummaryTest {

        private static Tp T(long euro, short cent) =>
            new() { DateTime = DateTime.Now, Amount = new Money(euro, cent), Comment = "" };

        [Fact]
        public void Empty_IsZero() {
            CoffeeFundSummary.Result r = CoffeeFundSummary.Summarize([]);
            Assert.Equal(0, r.Balance.ToTotalCents());
            Assert.Equal(0, r.Income.ToTotalCents());
            Assert.Equal(0, r.Expense.ToTotalCents());
        }

        [Fact]
        public void InAndOut_AreSummedExactly() {
            CoffeeFundSummary.Result r = CoffeeFundSummary.Summarize([T(10, 5), T(0, 30), T(-3, -20), T(0, -5)]);
            Assert.Equal(1035, r.Income.ToTotalCents());
            Assert.Equal(325, r.Expense.ToTotalCents());
            Assert.Equal(710, r.Balance.ToTotalCents());
        }

        [Fact]
        public void ExpenseAboveIncome_GivesNegativeBalance() {
            CoffeeFundSummary.Result r = CoffeeFundSummary.Summarize([T(1, 0), T(-2, -50)]);
            Assert.Equal(-150, r.Balance.ToTotalCents());
            Assert.Equal("-1,50 €", r.Balance.ToString());
        }
    }
}
