using CollateralPortal.Application.Common.Models;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralRegister;

public sealed record GetCollateralRegisterQuery(
    string? Search = null,
    string? StageFilter = null,
    string? TypeFilter = null,
    string? StatusFilter = null,
    string? TatFilter = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<CollateralRegisterItemVm>>;

public sealed record CollateralRegisterItemVm(
    Guid Id,
    string LoanContractId,
    string CustomerName,
    string Type,
    string CurrentStage,
    string Status,
    DateTime StageStartedAtUtc,
    DateTime CreatedAtUtc,
    string? TatStatus,
    double? TatElapsedHours,
    int? TatAllowedHours,
    DateTime? TatDueAtUtc);