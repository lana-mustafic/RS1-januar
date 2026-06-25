using Market.Infrastructure.Common;
using Market.Shared.Dtos;
using Market.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Market.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPI(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // Controllers + uniform BadRequest
        services.AddControllers()
            .ConfigureApiBehaviorOptions(opts =>
            {
                opts.InvalidModelStateResponseFactory = ctx =>
                {
                    var msg = string.Join("; ",
                        ctx.ModelState.Values.SelectMany(v => v.Errors)
                                             .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                                                 ? "Validation error"
                                                 : e.ErrorMessage));
                    return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new ErrorDto
                    {
                        Code = "validation.failed",
                        Message = msg
                    });
                };
            });

        // Typed options + validation on startup
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // JWT auth (reads from IOptions<JwtOptions>)
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer((o) =>
        {
            var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

            o.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(o =>
        {
            // Fallback policy requires authenticated user
            o.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // AdminOnly policy
            o.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

            // Staff policy (Admin, Manager, Employee)
            o.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Manager", "Employee"));
        });

        // Swagger with Bearer auth
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Market API", Version = "v1" });
            var xml = Path.Combine(AppContext.BaseDirectory, "Market.API.xml");
            if (File.Exists(xml))
                c.IncludeXmlComments(xml, includeControllerXmlComments: true);

            var bearer = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Unesi JWT token. Format: **Bearer {token}**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };
            c.AddSecurityDefinition("Bearer", bearer);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearer, Array.Empty<string>() } });
        });

        services.AddExceptionHandler<MarketExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
