using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitValuation;

/// <summary>
/// Coverage ratio thresholds from the BRD. Kept as a small pure function so
/// it's easy to move into configuration later without touching handlers.
/// </summary>
public static class CoverageRatioRules
{
    public static decimal RequiredPercent(CollateralType type, Region region) => type switch
    {
        CollateralType.RealEstate => region is Region.Lagos or Region.Abuja ? 130m : 150m,
        CollateralType.Cash => 110m,
        CollateralType.BlueChipShares => 200m,
        CollateralType.MovableAsset => 130m,
        CollateralType.Securities => 150m, // default until confirmed; align with Legal/Risk sign-off
        _ => 130m
    };
}
