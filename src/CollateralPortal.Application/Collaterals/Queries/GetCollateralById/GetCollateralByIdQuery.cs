using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralById;

public sealed record GetCollateralByIdQuery(Guid CollateralId)
    : IRequest<CollateralDetailVm>;
public sealed record CollateralDetailVm(
    Guid Id,

    // Loan / Customer
    string LoanContractId,
    string CustomerId,
    string CustomerName,
    string AccountNumber,
    decimal ApprovedLimit,

    // Collateral
    string Type,
    string Region,
    string Description,
    string? FacilityType,
    string Currency,
    string? CollateralAddress,
    string? CollateralOwner,
    string? BranchName,
    string? RelationshipManagerName,
    string? BranchManagerName,

    // Workflow
    string CurrentStage,
    string Status,
    DateTime StageStartedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc,

    // TAT
    string? TatStatus,
    double? TatElapsedHours,
    int? TatAllowedHours,
    DateTime? TatDueAtUtc,

    string? AdditionalInformationRequest,
    DateTime? AdditionalInformationRequestedAtUtc

);