using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralWorkItem;

public class GetCollateralWorkItemQueryHandler : IRequestHandler<GetCollateralWorkItemQuery, CollateralWorkItemVm>
{
    private readonly IApplicationDbContext _db;

    public GetCollateralWorkItemQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CollateralWorkItemVm> Handle(GetCollateralWorkItemQuery request, CancellationToken ct)
    {
        var c = await _db.Collaterals
            .Include(x => x.Valuation)
            .Include(x => x.Perfection)
            .Include(x => x.Insurance)
            .Include(x => x.Release)
            .FirstOrDefaultAsync(x => x.Id == request.CollateralId, ct)
            ?? throw new NotFoundException(nameof(Collateral), request.CollateralId);

        var collateralData = new CollateralDataTabVm(
            c.LoanContractId, c.AccountNumber, c.CustomerId, c.CustomerName, c.BranchName,
            c.FacilityType, c.Currency, c.Type.ToString(), c.Description, c.Region.ToString(),
            c.ApprovedLimit, c.CollateralAddress, c.CollateralOwner, c.RelationshipManagerName, c.BranchManagerName);

        var valuation = c.Valuation is null ? null : new ValuationTabVm(
            c.Valuation.ManualEntry, c.Valuation.OpenMarketValue, c.Valuation.ForcedSaleValue,
            c.Valuation.ValuersName, c.Valuation.ValuationDate, c.Valuation.ActualCoverageRatioPercent,
            c.Valuation.RequiredCoverageRatioPercent, c.Valuation.CoverageAdequate,
            c.Valuation.Decision.ToString(), c.Valuation.Remark);

        var perfection = c.Perfection is null ? null : new PerfectionTabVm(
            c.Perfection.RegistrationReference, c.Perfection.LegalUnit, c.Perfection.Stage.ToString(),
            c.Perfection.QueryNotes, c.Perfection.CollateralValid, c.Perfection.InvalidReason);

        var insurance = c.Insurance is null ? null : new InsuranceTabVm(
            c.Insurance.PolicyNumber, c.Insurance.Insurer, c.Insurance.CoverageAmount, c.Insurance.PremiumAmount,
            c.Insurance.PolicyStartDate, c.Insurance.PolicyExpiryDate, c.Insurance.RenewalCadence.ToString(),
            c.Insurance.RenewalReminderSent, c.Insurance.OverdueNoticeSent);

        var release = c.Release is null ? null : new ReleaseTabVm(
            c.Release.LoanFullyRepaid, c.Release.ReleaseDate, c.Release.ReleasedBy, c.Release.Notes);

        return new CollateralWorkItemVm(c.Id, c.CurrentStage.ToString(), c.Status.ToString(), collateralData, valuation, perfection, insurance, release);
    }
}
