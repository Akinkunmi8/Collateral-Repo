using Microsoft.AspNetCore.Authorization;

namespace CollateralPortal.Api.Authorization;

public class DashboardAccessRequirement : IAuthorizationRequirement
{
    public DashboardModule Module { get; }
    public DashboardAccessLevel MinimumLevel { get; }

    public DashboardAccessRequirement(DashboardModule module, DashboardAccessLevel minimumLevel)
    {
        Module = module;
        MinimumLevel = minimumLevel;
    }

    /// <summary>Policy name convention used both when registering and when applying [Authorize(Policy = ...)].</summary>
    public static string PolicyName(DashboardModule module, DashboardAccessLevel level) => $"Dashboard.{module}.{level}";
}
