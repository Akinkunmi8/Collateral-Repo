using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Application.Common.Models;

public sealed record CollateralSourceDto(
    string LoanContractId,

    CollateralType CollateralType,
    Region Region,
    string CollateralDescription,

    string? FacilityType,
    string Currency,
    string? CollateralAddress,
    string? CollateralOwner,
    string? RelationshipManagerName,
    string? BranchManagerName);