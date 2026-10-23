using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralById;

public sealed class GetCollateralByIdQueryHandler
    : IRequestHandler<GetCollateralByIdQuery, CollateralDetailVm>
{
    private readonly IApplicationDbContext _db;
    private readonly ITatService _tatService;

    public GetCollateralByIdQueryHandler(
        IApplicationDbContext db,
        ITatService tatService)
    {
        _db = db;
        _tatService = tatService;
    }

    public async Task<CollateralDetailVm> Handle(
        GetCollateralByIdQuery request,
        CancellationToken ct)
    {
        // ==================================================
        // LOAD COLLATERAL
        // ==================================================

        var collateral = await _db.Collaterals
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == request.CollateralId,
                ct)
            ?? throw new NotFoundException(
                nameof(Collateral),
                request.CollateralId);

        // ==================================================
        // TAT
        // ==================================================

        var allowedHours =
            _tatService.GetAllowedHours(
                collateral.CurrentStage);

        string? tatStatus = null;
        double? tatElapsedHours = null;
        DateTime? tatDueAtUtc = null;

        if (allowedHours > 0)
        {
            var now = DateTime.UtcNow;

            tatStatus =
                _tatService.GetStatus(
                    collateral.CurrentStage,
                    collateral.StageStartedAtUtc,
                    now)
                .ToString();

            tatElapsedHours =
                Math.Round(
                    _tatService.GetElapsedHours(
                        collateral.StageStartedAtUtc,
                        now),
                    2);

            tatDueAtUtc =
                _tatService.GetDueDateUtc(
                    collateral.CurrentStage,
                    collateral.StageStartedAtUtc);
        }

        // ==================================================
        // LATEST ADDITIONAL INFORMATION REQUEST
        // ==================================================
        //
        // This must NOT be inside the TAT block.
        //
        // Additional information may need to be displayed
        // regardless of whether the current workflow stage
        // has an active TAT.
        // ==================================================

        var additionalInfoRequest =
            await _db.AuditLogs
                .AsNoTracking()
                .Where(x =>
                    x.CollateralId == collateral.Id &&
                    x.Action ==
                        "AdditionalCollateralRequested")
                .OrderByDescending(
                    x => x.PerformedAtUtc)
                .Select(x => new
                {
                    x.Details,
                    x.PerformedAtUtc
                })
                .FirstOrDefaultAsync(ct);

        // ==================================================
        // RESPONSE
        // ==================================================

        return new CollateralDetailVm(
            Id:
                collateral.Id,

            // Customer / Loan
            LoanContractId:
                collateral.LoanContractId,

            CustomerId:
                collateral.CustomerId,

            CustomerName:
                collateral.CustomerName,

            AccountNumber:
                collateral.AccountNumber,

            ApprovedLimit:
                collateral.ApprovedLimit,

            // Collateral
            Type:
                collateral.Type.ToString(),

            Region:
                collateral.Region.ToString(),

            Description:
                collateral.Description,

            FacilityType:
                collateral.FacilityType,

            Currency:
                collateral.Currency,

            CollateralAddress:
                collateral.CollateralAddress,

            CollateralOwner:
                collateral.CollateralOwner,

            BranchName:
                collateral.BranchName,

            RelationshipManagerName:
                collateral.RelationshipManagerName,

            BranchManagerName:
                collateral.BranchManagerName,

            // Workflow
            CurrentStage:
                collateral.CurrentStage.ToString(),

            Status:
                collateral.Status.ToString(),

            StageStartedAtUtc:
                collateral.StageStartedAtUtc,

            CreatedAtUtc:
                collateral.CreatedAtUtc,

            LastModifiedAtUtc:
                collateral.LastModifiedAtUtc,

            // TAT
            TatStatus:
                tatStatus,

            TatElapsedHours:
                tatElapsedHours,

            TatAllowedHours:
                allowedHours > 0
                    ? allowedHours
                    : null,

            TatDueAtUtc:
                tatDueAtUtc,

            // Additional Information
            AdditionalInformationRequest:
                additionalInfoRequest?.Details,

            AdditionalInformationRequestedAtUtc:
                additionalInfoRequest?.PerformedAtUtc
        );
    }
}