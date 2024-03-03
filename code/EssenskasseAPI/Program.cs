using Serilog;
using Serilog.Events;
using Serilog.Sinks.LiteDB;

namespace EssenskasseAPI {

    public class Program {

        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Allow CORS
            app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.LiteDB(
                    databaseUrl: AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB/Logs.litedb",
                    rollingFilePeriod: RollingPeriod.Weekly
                    )
                .CreateLogger();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            try {
                Log.Information("Starting the application...");
                app.Run();
            }
            catch (Exception ex) {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally {
                Log.Information("Shutting down the application.");
                Log.CloseAndFlush();
            }
        }
    }
}
