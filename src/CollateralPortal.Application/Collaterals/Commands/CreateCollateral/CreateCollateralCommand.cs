using CollateralPortal.Domain.Enums;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.CreateCollateral;

public record CreateCollateralCommand(
    string LoanContractId,
    CollateralType Type,
    Region Region,
    string Description,

    // Additional fields needed for the "Notification on collateral queue"
    // email — captured here whether the data came via auto-pull or manual entry.
    string? FacilityType,
    string Currency,
    string? CollateralAddress,
    string? CollateralOwner,
    string? RelationshipManagerName,
    string? BranchManagerName) : IRequest<Guid>;
