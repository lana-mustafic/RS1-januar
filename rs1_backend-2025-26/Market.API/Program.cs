using Market.API;
using Market.API.Middleware;
using Market.Application;
using Market.Infrastructure;
using Serilog;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        //
        // 0) Bootstrap logger (very early, no full config yet)
        //
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() // minimal sink so we see startup errors
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Market API...");

            //
            // 1) Standard builder (includes appsettings.json, appsettings.{ENV}.json,
            //    environment variables, user-secrets (Dev), and command-line args)
            //
            var builder = WebApplication.CreateBuilder(args);

            // 2) Promote Serilog to full configuration from builder.Configuration
            //    (reads "Serilog" section from appsettings + ENV overrides)
            //
            builder.Host.UseSerilog((ctx, services, cfg) =>
            {
                cfg.ReadFrom.Configuration(ctx.Configuration)   // Serilog section in appsettings
                   .ReadFrom.Services(services)                 // DI enrichers if any
                   .Enrich.FromLogContext()
                   .Enrich.WithThreadId()
                   .Enrich.WithProcessId()
                   .Enrich.WithMachineName();
            });

            // Optional: remove default providers to have only Serilog
            builder.Logging.ClearProviders();

            // ---------------------------------------------------------
            // 3. Layer registrations
            // ---------------------------------------------------------
            builder.Services
                .AddAPI(builder.Configuration, builder.Environment)
                .AddInfrastructure(builder.Configuration, builder.Environment)
                .AddApplication();

            // CORS policy to allow Angular dev server access
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin",
                    policy =>
                    {
                        policy
                            .SetIsOriginAllowed(_=> true) // allow any origin
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            var app = builder.Build();

            // ---------------------------------------------------------
            // 4. Middleware pipeline
            // ---------------------------------------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Global exception handler (IExceptionHandler)
            app.UseExceptionHandler();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseHttpsRedirection();
            // UseCors ide prije UseAuthorization i UseAuthentification
            app.UseCors("AllowAnyOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Database migrations + seeding
            await app.Services.InitializeDatabaseAsync(app.Environment);

            Log.Information("Market API started successfully.");
            app.Run();
        }
        catch (HostAbortedException)
        {
            // EF Core tools abortiraju host nakon što uzmu DbContext.
            // Ovo nije runtime greška – samo tiho izađi.
            Log.Information("Host aborted by EF Core tooling (design-time) - its ok.");
        }
        catch (Exception ex)
        {
            // Any startup failure will be logged here
            Log.Fatal(ex, "Market API terminated unexpectedly.");
        }
        finally
        {
            // Ensure all logs are flushed before the app exits
            Log.CloseAndFlush();
        }
    }
}
