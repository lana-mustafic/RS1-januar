using Market.Application.Abstractions;
using Market.Application.Abstractions.Caching;
using Market.Infrastructure.Caching;
using Market.Infrastructure.Common;
using Market.Infrastructure.Database;
using Market.Shared.Constants;
using Market.Shared.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Market.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // Typed ConnectionStrings + validation
        services.AddOptions<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // DbContext: InMemory for test environments; SQL Server otherwise
        services.AddDbContext<DatabaseContext>((sp, options) =>
        {
            if (env.IsTest())
            {
                options.UseInMemoryDatabase("IntegrationTestsDb");

                return;
            }

            var cs = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Main;
            options.UseSqlServer(cs);
        });

        // IAppDbContext mapping
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<DatabaseContext>());

        // Identity hasher
        services.AddScoped<IPasswordHasher<MarketUserEntity>, PasswordHasher<MarketUserEntity>>();

        // Token service (reads JwtOptions via IOptions<JwtOptions>)
        services.AddTransient<IJwtTokenService, JwtTokenService>();

        // HttpContext accessor + current user
        services.AddHttpContextAccessor();
        services.AddScoped<IAppCurrentUser, AppCurrentUser>();

        // TimeProvider (if used in handlers/services)
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        // Redis - StackExchange.Redis + IDistributedCache
        var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            //// IConnectionMultiplexer for atomic operations
            //services.AddSingleton<IConnectionMultiplexer>(sp =>
            //    ConnectionMultiplexer.Connect(redisConnectionString));

            //// IDistributedCache for general caching
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = redisConnectionString;
            //    options.InstanceName = configuration.GetValue<string>("Redis:InstanceName") ?? "Market:";
            //});

            // Cache services
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<ICatalogCacheVersionService, CatalogCacheVersionService>();
        }

        return services;
    }
}
