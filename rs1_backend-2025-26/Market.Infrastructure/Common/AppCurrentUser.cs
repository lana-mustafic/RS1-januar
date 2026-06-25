using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Market.Application.Abstractions;

namespace Market.Infrastructure.Common;

/// <summary>
/// Implementation of IAppCurrentUser that reads data from a JWT token.
/// </summary>
public sealed class AppCurrentUser(IHttpContextAccessor httpContextAccessor)
    : IAppCurrentUser
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public int? UserId =>
        int.TryParse(_user?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public string? Email =>
        _user?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated =>
        _user?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin =>
        _user?.FindFirstValue("is_admin")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

    public bool IsManager =>
        _user?.FindFirstValue("is_manager")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

    public bool IsEmployee =>
        _user?.FindFirstValue("is_employee")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
}