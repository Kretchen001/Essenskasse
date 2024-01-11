using LiteDB;
using Modells;

namespace EssenskasseAPI.LiteDB {

    /// <summary></summary>
    public class SaldoDbController {

        #region Config
        /// <summary>Return the location, where the database is located.</summary>
        private static string SaveDir {
            get {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/data.liteDB")) {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB")) {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB");
                    }
                    File.Create(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/data.liteDB").Close();
                }
                return (AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/data.liteDB");
            }
        }

        /// <summary>Define the name of the table.</summary>
        private readonly string tableNamePayment = "Payment";
        private readonly string tableNameOrder = "Order";
        #endregion

        #region Function
        /// <summary>This load all registered person, recalculate
        /// the saldo and update finaly the person in the database.</summary>
        public void RecalculateSaldoInTheDatabase() {
            PersonDbController _personDbController = new PersonDbController();
            List<Person> persons = _personDbController.RequestAllPerson();

            foreach (Person x in persons) {
                Money paymentSaldo = GetAllPaymentsFromPerson(x.Guid);
                Money orderSaldo = GetAllOrdersFromPerson(x.Guid);
                Money saldo = new Money().CalculateMoney(CalculationOperation.Substraction, paymentSaldo, orderSaldo)!;
                x.Saldo = saldo;
                _personDbController.UpdatePerson(x);
            }
        }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Money GetAllPaymentsFromPerson(Guid guid) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Payment> col = db.GetCollection<Payment>(tableNamePayment);

                List<Payment> list = new List<Payment>();

                IEnumerator<Payment> enumerator = col.FindAll().GetEnumerator();
                while (enumerator.MoveNext()) {
                    Payment currentPayment = enumerator.Current;
                    if (currentPayment.ReferencedPerson.Equals(guid)) {
                        list.Add(currentPayment);
                    }
                }
                enumerator.Dispose();

                Money money = new Money();
                foreach (Payment x in list) {
                    money.Euro += x.Amount.Euro;
                    money.Cent += x.Amount.Cent;
                }
                while (money.Cent > 100) {
                    money.Cent -= 100;
                    money.Euro += 1;
                }
                return money;
            }
        }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Money GetAllOrdersFromPerson(Guid guid) {
            using (LiteDatabase db = new LiteDatabase(SaveDir)) {
                ILiteCollection<Order> col = db.GetCollection<Order>(tableNameOrder);

                List<Order> list = new List<Order>();

                IEnumerator<Order> enumerator = col.FindAll().GetEnumerator();
                while (enumerator.MoveNext()) {
                    Order currentOrder = enumerator.Current;
                    if (currentOrder.ReferencedPerson.Equals(guid)) {
                        list.Add(currentOrder);
                    }
                }
                enumerator.Dispose();

                Money money = new Money();
                foreach (Order x in list) {
                    money.Euro += x.Money.Euro;
                    money.Cent += x.Money.Cent;
                }
                while (money.Cent > 100) {
                    money.Cent -= 100;
                    money.Euro += 1;
                }
                return money;
            }
        }
        #endregion
    }
}
