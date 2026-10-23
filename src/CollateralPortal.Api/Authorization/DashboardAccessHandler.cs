using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CollateralPortal.Api.Authorization;

public class DashboardAccessHandler : AuthorizationHandler<DashboardAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, DashboardAccessRequirement requirement)
    {
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var level = DashboardPermissionMatrix.GetAccessLevel(roles, requirement.Module);

        if (level >= requirement.MinimumLevel && level != DashboardAccessLevel.None)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
