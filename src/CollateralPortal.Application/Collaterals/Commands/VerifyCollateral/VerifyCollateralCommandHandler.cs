using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Commands.VerifyCollateral;

public sealed class VerifyCollateralCommandHandler
    : IRequestHandler<VerifyCollateralCommand>
{
    private readonly IApplicationDbContext _db;

    public VerifyCollateralCommandHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(
        VerifyCollateralCommand request,
        CancellationToken cancellationToken)
    {
        var collateral =
            await _db.Collaterals
                .FirstOrDefaultAsync(
                    x => x.Id == request.CollateralId,
                    cancellationToken);

        if (collateral is null)
        {
            throw new KeyNotFoundException(
                $"Collateral '{request.CollateralId}' was not found.");
        }

        // Only records waiting for collateral approval
        // are allowed through this operation.
        if (collateral.CurrentStage != WorkflowStage.CollateralData ||
            collateral.Status != CollateralStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                "Only collateral records pending approval can be verified.");
        }

        switch (request.Decision)
        {
            case CollateralVerificationDecision.Approve:
                {
                    collateral.MoveToValuation();

                    _db.AuditLogs.Add(
                        new AuditLog
                        {
                            CollateralId = collateral.Id,

                            Action =
                                "CollateralVerified",

                            PerformedBy =
                                "system",

                            PerformedAtUtc =
                                DateTime.UtcNow,

                            Details =
                                BuildDetails(
                                    "Collateral verified and approved. " +
                                    "Record moved to Valuation.",
                                    request.Remark)
                        });

                    break;
                }

            case CollateralVerificationDecision
                .RequestAdditionalCollateral:
                {
                    collateral.RequestAdditionalCollateral();

                    _db.AuditLogs.Add(
                        new AuditLog
                        {
                            CollateralId =
                                collateral.Id,

                            Action =
                                "AdditionalCollateralRequested",

                            PerformedBy =
                                "system",

                            PerformedAtUtc =
                                DateTime.UtcNow,

                            Details =
                                BuildDetails(
                                    "Additional collateral/information requested.",
                                    request.Remark)
                        });

                    break;
                }

            default:
                throw new InvalidOperationException(
                    "Unsupported collateral verification decision.");
        }

        await _db.SaveChangesAsync(
            cancellationToken);
    }

    private static string BuildDetails(
        string message,
        string? remark)
    {
        if (string.IsNullOrWhiteSpace(remark))
            return message;

        return $"{message} Remark: {remark.Trim()}";
    }
}