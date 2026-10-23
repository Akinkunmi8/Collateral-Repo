using CollateralPortal.Application.Common.Constants;
using CollateralPortal.Infrastructure.Identity;

namespace CollateralPortal.Api.Authorization;

/// <summary>
/// Role -> dashboard -> access-level matrix, per the access rules given for
/// the portal:
///   - CreditTeam / ValuationTeam / CollateralReleaseTeam: write access to
///     CollateralData, Valuation and Release; read-only on Legal and
///     Insurance; no access to Audit or Admin.
///   - LegalTeam: write access to the Legal dashboard only.
///   - InsuranceTeam: write access to the Insurance dashboard only.
///   - Admin: full access everywhere, including Audit and Admin.
/// Roles not listed here (e.g. the notification-only RM/BM/AM/RH/SgHead
/// roles) default to no dashboard access until a rule is added for them.
/// </summary>
public static class DashboardPermissionMatrix
{
    private static readonly Dictionary<DashboardModule, DashboardAccessLevel> CollateralTeamAccess = new()
    {
        [DashboardModule.CollateralData] = DashboardAccessLevel.Write,
        [DashboardModule.Valuation] = DashboardAccessLevel.Write,
        [DashboardModule.Release] = DashboardAccessLevel.Write,
        [DashboardModule.Legal] = DashboardAccessLevel.Read,
        [DashboardModule.Insurance] = DashboardAccessLevel.Read,
        [DashboardModule.Audit] = DashboardAccessLevel.None,
        [DashboardModule.Admin] = DashboardAccessLevel.None
    };

    private static readonly Dictionary<string, Dictionary<DashboardModule, DashboardAccessLevel>> Matrix = new()
    {
        [Roles.CreditTeam] = CollateralTeamAccess,
        [Roles.ValuationTeam] = CollateralTeamAccess,
        //[Roles.CollateralReleaseTeam] = CollateralTeamAccess,

        [Roles.LegalTeam] = new()
        {
            [DashboardModule.Legal] = DashboardAccessLevel.Write
            // everything else defaults to None — Legal Team only sees the Legal dashboard.
        },

        [Roles.InsuranceTeam] = new()
        {
            [DashboardModule.Insurance] = DashboardAccessLevel.Write
            // everything else defaults to None — Insurance Team only sees the Insurance dashboard.
        },

        [Roles.Auditor] = Enum.GetValues<DashboardModule>().ToDictionary(m => m, m => m == DashboardModule.Admin ? DashboardAccessLevel.None : DashboardAccessLevel.Read),
        [Roles.Admin] = Enum.GetValues<DashboardModule>().ToDictionary(m => m, _ => DashboardAccessLevel.Write)
    };

    /// <summary>Highest access level granted to any of the given roles for the given dashboard.</summary>
    public static DashboardAccessLevel GetAccessLevel(IEnumerable<string> userRoles, DashboardModule module)
    {
        var best = DashboardAccessLevel.None;

        foreach (var role in userRoles)
        {
            if (Matrix.TryGetValue(role, out var perModule) &&
                perModule.TryGetValue(module, out var level) &&
                level > best)
            {
                best = level;
            }
        }

        return best;
    }
}
