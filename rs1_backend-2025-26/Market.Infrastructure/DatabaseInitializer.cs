using Market.Infrastructure.Database;
using Market.Infrastructure.Database.Seeders;
using Market.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Market.Infrastructure;

public static class DatabaseInitializer
{
    /// <summary>
    /// Centralized migration and seeding.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, IHostEnvironment env)
    {
        await using var scope = services.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        if (env.IsTest())
        {
            await ctx.Database.EnsureCreatedAsync();
            await DynamicDataSeeder.SeedAsync(ctx);
            return;
        }

        // SQL Server or similar
        await ctx.Database.MigrateAsync();//update-database

        if (env.IsDevelopment())
        {
            await DynamicDataSeeder.SeedAsync(ctx);
        }
    }
}