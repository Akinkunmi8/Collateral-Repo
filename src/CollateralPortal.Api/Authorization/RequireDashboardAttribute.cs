using Microsoft.AspNetCore.Authorization;

namespace CollateralPortal.Api.Authorization;

/// <summary>
/// Convenience wrapper so controllers can write
/// [RequireDashboard(DashboardModule.Legal, DashboardAccessLevel.Write)]
/// instead of the raw policy-name string.
/// </summary>
public class RequireDashboardAttribute : AuthorizeAttribute
{
    public RequireDashboardAttribute(DashboardModule module, DashboardAccessLevel level)
        : base(DashboardAccessRequirement.PolicyName(module, level)) { }
}
