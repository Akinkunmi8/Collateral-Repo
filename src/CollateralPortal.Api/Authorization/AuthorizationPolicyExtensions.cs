using Microsoft.Extensions.DependencyInjection;

namespace CollateralPortal.Api.Authorization;

public static class AuthorizationPolicyExtensions
{
    /// <summary>
    /// Registers one policy per (module, level) combination — "Dashboard.Legal.Write",
    /// "Dashboard.Insurance.Read", etc. — so controllers can reference them by name
    /// via [Authorize(Policy = ...)] without hand-registering each one.
    /// </summary>
    public static IServiceCollection AddDashboardAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, DashboardAccessHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var module in Enum.GetValues<DashboardModule>())
            {
                foreach (var level in new[] { DashboardAccessLevel.Read, DashboardAccessLevel.Write })
                {
                    var policyName = DashboardAccessRequirement.PolicyName(module, level);
                    options.AddPolicy(policyName, policy =>
                        policy.Requirements.Add(new DashboardAccessRequirement(module, level)));
                }
            }
        });

        return services;
    }
}
