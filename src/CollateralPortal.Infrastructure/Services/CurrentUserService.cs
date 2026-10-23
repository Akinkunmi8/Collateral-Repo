using CollateralPortal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CollateralPortal.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public string? UserId { get; }
    public string? UserName { get; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        UserId = user?.FindFirst("sub")?.Value;
        UserName = user?.Identity?.Name;
    }
}
