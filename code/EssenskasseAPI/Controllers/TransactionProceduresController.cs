using EssenskasseAPI.LiteDB;
using Microsoft.AspNetCore.Mvc;
using Modells;
using SerilogTimings;

namespace EssenskasseAPI.Controllers {

    [ApiController, Route("api/transactionprocedure")]
    public class TransactionProceduresController : ControllerBase {

        readonly TransactionProceduresDbController _transactionProcedureController = new TransactionProceduresDbController();

        [HttpGet, Route("all")]
        public IActionResult GetAllProcedures() {
            using (Operation.Time("TaskDuration")) {
                return Ok(_transactionProcedureController.GetAll());
            }
        }

        [HttpPost, Route("new")]
        public IActionResult NewOne([FromBody] TransactionProcedure newOne) {
            using (Operation.Time("TaskDuration")) {
                return Ok(_transactionProcedureController.NewTransactionProcedure(newOne));
            }
        }
    }
}
