using CollateralPortal.Application.Collaterals.Commands.SubmitValuation;
using CollateralPortal.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CollateralPortal.UnitTests;

public class CoverageRatioRulesTests
{
    [Theory]
    [InlineData(CollateralType.RealEstate, Region.Lagos, 130)]
    [InlineData(CollateralType.RealEstate, Region.Abuja, 130)]
    [InlineData(CollateralType.RealEstate, Region.OtherRegion, 150)]
    [InlineData(CollateralType.Cash, Region.Lagos, 110)]
    [InlineData(CollateralType.BlueChipShares, Region.Lagos, 200)]
    [InlineData(CollateralType.MovableAsset, Region.OtherRegion, 130)]
    public void RequiredPercent_matches_BRD_thresholds(CollateralType type, Region region, decimal expected)
    {
        CoverageRatioRules.RequiredPercent(type, region).Should().Be(expected);
    }
}
