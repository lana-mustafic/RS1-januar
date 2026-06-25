using Market.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Market.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR only from the Application layer
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation from the Application layer
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline behaviors (npr. ValidationBehavior)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // TimeProvider — if used by handlers
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}