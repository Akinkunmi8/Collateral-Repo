using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Commands.ResubmitCollateral;

public sealed class ResubmitCollateralCommandHandler
    : IRequestHandler<ResubmitCollateralCommand>
{
    private readonly IApplicationDbContext _db;

    public ResubmitCollateralCommandHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(
        ResubmitCollateralCommand request,
        CancellationToken cancellationToken)
    {
        var collateral = await _db.Collaterals
            .FirstOrDefaultAsync(
                x => x.Id == request.CollateralId,
                cancellationToken);

        if (collateral is null)
        {
            throw new KeyNotFoundException(
                $"Collateral '{request.CollateralId}' was not found.");
        }

        if (collateral.CurrentStage !=
                Domain.Enums.WorkflowStage.CollateralData ||
            collateral.Status !=
                Domain.Enums.CollateralStatus.AdditionalCollateralRequested)
        {
            throw new InvalidOperationException(
                "Only collateral awaiting additional information can be resubmitted.");
        }

        collateral.ResubmitForVerification();

        _db.AuditLogs.Add(
            new AuditLog
            {
                CollateralId = collateral.Id,

                Action = "CollateralResubmitted",

                // Temporary until we wire authenticated
                // user context into audit.
                PerformedBy = "system",

                PerformedAtUtc = DateTime.UtcNow,

                Details =
                    $"Collateral resubmitted for verification. Response: {request.Response.Trim()}"
            });

        await _db.SaveChangesAsync(
            cancellationToken);
    }
}