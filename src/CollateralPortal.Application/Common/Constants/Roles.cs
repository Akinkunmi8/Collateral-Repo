namespace CollateralPortal.Application.Common.Constants;

/// <summary>
/// Roles mirror the actors in the BRD process flow / workflow swimlanes,
/// so authorization on each workflow endpoint maps directly back to the
/// "who does this step" column in the process diagram.
/// </summary>
public static class Roles
{
    public const string CreditTeam = "CreditTeam";
    public const string ValuationTeam = "ValuationTeam";
    public const string LegalTeam = "LegalTeam";
    public const string InsuranceTeam = "InsuranceTeam";
    //public const string CollateralReleaseTeam = "CollateralReleaseTeam";

    // Notification recipients from the BRD (RM/BM/AM/RH/SG Head) — included
    // as roles too, so they can log in to view/approve rather than only
    // receive email alerts.
    public const string RelationshipManager = "RelationshipManager";
    public const string BranchManager = "BranchManager";
    //public const string AccountManager = "AccountManager";
    public const string RegionalHead = "RegionalHead";
    //public const string SgHead = "SgHead";

    public const string Admin = "Admin";
    public const string Auditor = "Auditor";

    public static readonly string[] All =
    {
        CreditTeam, ValuationTeam, LegalTeam, InsuranceTeam,
        RelationshipManager, BranchManager,RegionalHead,Admin, Auditor
    };
}
