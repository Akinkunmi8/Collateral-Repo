namespace CollateralPortal.Domain.Enums;

/// <summary>
/// The Valuation Team's decision dropdown per the workstep UI — user-driven,
/// informed by (not dictated by) the computed coverage ratio.
/// </summary>
public enum ValuationDecision
{
    Submit = 1,
    RequestAdditionalValuationInformation = 2
}
