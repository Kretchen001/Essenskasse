using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

namespace EssenskasseAPI {

    public class Program {

        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(x => {
                x.SwaggerDoc("v1", new OpenApiInfo { Title = "EssenskasseAPI", Version = "v1" });
                // "Authorize"-Knopf: Token aus POST api/user/newTokenForUser eintragen (ohne Anfuehrungszeichen),
                // dann senden alle "Try it out"-Aufrufe es als "Authorization: Bearer ..." mit.
                x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                    Description = "Sitzungs-Token (nur das Token, ohne \"Bearer \" davor)",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                x.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                        Array.Empty<string>()
                    }
                });
            });

            WebApplication app = builder.Build();

            // Swagger ist standardmaessig an (auch im Container). Abschalten mit "Swagger:Enabled": false
            // (z. B. Umgebungsvariable Swagger__Enabled=false).
            if (app.Configuration.GetValue<bool>("Swagger:Enabled", true)) {
                app.UseSwagger();
                app.UseSwaggerUI(x => {
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "EssenskasseAPI");
                });
            }

            // CORS: Mit "Cors:AllowedOrigins" (Liste, z. B. Cors__AllowedOrigins__0=http://meinserver:8009)
            // wird auf die PWA eingeschraenkt. Ohne Angabe bleibt es wie bisher offen; das ist vertretbar,
            // weil die API Anmeldedaten nur per Authorization-Header (kein Cookie) entgegennimmt.
            string[] allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            app.UseCors(x => {
                x.AllowAnyMethod().AllowAnyHeader();
                if (allowedOrigins.Length > 0) {
                    x.WithOrigins(allowedOrigins);
                }
                else {
                    x.AllowAnyOrigin();
                }
            });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: "Log/Logs.txt",
                    rollingInterval: RollingInterval.Day
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
