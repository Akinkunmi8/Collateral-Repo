using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using CollateralPortal.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetReleaseRepaymentStatus;

public sealed class GetReleaseRepaymentStatusQueryHandler
    : IRequestHandler<
        GetReleaseRepaymentStatusQuery,
        ReleaseRepaymentStatusVm>
{
    private readonly IApplicationDbContext _db;
    private readonly ICoreBankingClient _coreBankingClient;

    public GetReleaseRepaymentStatusQueryHandler(
        IApplicationDbContext db,
        ICoreBankingClient coreBankingClient)
    {
        _db = db;
        _coreBankingClient = coreBankingClient;
    }

    public async Task<ReleaseRepaymentStatusVm> Handle(
        GetReleaseRepaymentStatusQuery request,
        CancellationToken ct)
    {
        var collateral = await _db.Collaterals
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.CollateralId,
                ct)
            ?? throw new NotFoundException(
                nameof(Collateral),
                request.CollateralId);

        if (
            collateral.CurrentStage != WorkflowStage.SystemHold ||
            collateral.Status != CollateralStatus.OnHold
        )
        {
            throw new DomainException(
                "Repayment status can only be checked for collateral currently on System Hold.");
        }

        var facility =
            await _coreBankingClient.GetFacilityAsync(
                collateral.LoanContractId,
                ct);

        if (facility is null)
        {
            throw new DomainException(
                $"Unable to retrieve facility information for loan {collateral.LoanContractId}.");
        }

        return new ReleaseRepaymentStatusVm(
            CollateralId: collateral.Id,
            LoanContractId: facility.LoanContractId,
            OutstandingBalance: facility.OutstandingBalance,
            NextRepaymentDate: facility.NextRepaymentDate,
            FullyRepaid: facility.FullyRepaid,
            Branch: facility.Branch,
            Region: facility.Region);
    }
}