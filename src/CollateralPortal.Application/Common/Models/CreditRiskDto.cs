namespace CollateralPortal.Application.Common.Models;

/// <summary>Shape returned by the (mocked) Credit Risk Management System / CLOS.</summary>
public record CreditRiskDto(
    string CustomerId,
    string RiskRating,
    decimal ProbabilityOfDefaultPercent,
    DateTime RatingDate);
