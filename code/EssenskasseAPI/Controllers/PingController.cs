using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Serilog;
using SerilogTimings;
using System.Data;

namespace EssenskasseAPI.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase {

        private readonly IConfiguration Configuration;

        public PingController(IConfiguration configuration) {
            Configuration = configuration;
        }

        [HttpGet, Route("ping")]
        public IActionResult Ping() {
            using (Operation.Time("TaskDuration")) {
                Log.Information("Ping called.");
                return Ok();
            }
        }

        [HttpGet, Route("pingDb")]
        public IActionResult PingWithDb() {
            using(Operation.Time("TaskDuration")) {
                try {
                    string host = "localhost";
                    string userid = "dbuser";
                    string password = "password";
                    string port = "3306";
                    string userDatabase = "essenskasse";
                    MySqlConnection con = new MySqlConnection(
                        $"Server={host};" +
                        $"Port={port};" +
                        $"Uid={userid};" +
                        $"Pwd={password};" +
                        $"Database={userDatabase};"
                    );
                    //MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:essenskasse"]);

                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM Test", con);

                    con.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    reader.Close();
                    con.Close();

                    return Ok();
                }
                catch (Exception ex) {
                    return BadRequest(ex.ToString());
                }
            }
        }
    }
}
