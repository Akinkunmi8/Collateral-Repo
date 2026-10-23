using CollateralPortal.Application.Common.Models;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetValuationQueue;

public sealed record ValuationQueueItemVm(
    Guid Id,
    string LoanContractId,
    string CustomerId,
    string CustomerName,
    string AccountNumber,
    string Type,
    string Region,
    string Description,
    decimal ApprovedLimit,
    string Currency,
    string? FacilityType,
    string? CollateralAddress,
    string? CollateralOwner,
    string? RelationshipManagerName,
    string? BranchManagerName,
    string CurrentStage,
    string Status,
    DateTime StageStartedAtUtc,
    DateTime CreatedAtUtc,

    string? TatStatus,
    double? TatElapsedHours,
    int? TatAllowedHours,
    DateTime? TatDueAtUtc
);

public sealed record GetValuationQueueQuery(
    string? Search,
    string? Status,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<ValuationQueueItemVm>>;