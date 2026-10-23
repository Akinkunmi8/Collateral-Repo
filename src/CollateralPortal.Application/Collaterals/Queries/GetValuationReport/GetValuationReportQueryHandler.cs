using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Collaterals.Commands.SubmitValuation;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetValuationReport;

public sealed class GetValuationReportQueryHandler
    : IRequestHandler<GetValuationReportQuery, ValuationReportVm>
{
    private readonly IApplicationDbContext _db;
    private readonly IValuationPortalClient _valuationClient;

    public GetValuationReportQueryHandler(
        IApplicationDbContext db,
        IValuationPortalClient valuationClient)
    {
        _db = db;
        _valuationClient = valuationClient;
    }

    public async Task<ValuationReportVm> Handle(
        GetValuationReportQuery request,
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

        var canViewValuation =
        collateral.CurrentStage == WorkflowStage.Valuation &&
            (
                collateral.Status == CollateralStatus.InValuation ||
                collateral.Status ==
                    CollateralStatus.AdditionalValuationInformationRequested
            );

        if (!canViewValuation)
        {
            throw new InvalidOperationException(
                $"Collateral is not currently available for valuation. " +
                $"Current stage: {collateral.CurrentStage}; " +
                $"Status: {collateral.Status}.");
        }

        // IMPORTANT:
        // AccountNumber comes from our trusted Collateral record.
        var report =
            await _valuationClient.GetValuationReportAsync(
                collateral.AccountNumber,
                ct)
            ?? throw new InvalidOperationException(
                $"No completed valuation report was found for account {collateral.AccountNumber}.");

        var requiredPercent =
            CoverageRatioRules.RequiredPercent(
                collateral.Type,
                collateral.Region);

        var actualPercent =
            collateral.ApprovedLimit == 0 ||
            report.ForceSalesValue is null or 0
                ? 0
                : Math.Round(
                    report.ForceSalesValue.Value /
                    collateral.ApprovedLimit * 100m,
                    2);

        var adequate =
            actualPercent >= requiredPercent;

        return new ValuationReportVm(
            CollateralId: collateral.Id,

            LoanContractId:
                collateral.LoanContractId,

            AccountNumber:
                collateral.AccountNumber,

            CustomerName:
                collateral.CustomerName,

            ApprovedLimit:
                collateral.ApprovedLimit,

            Currency:
                collateral.Currency,

            FacilityRef:
                report.FacilityRef,

            FacilityAmount:
                report.FacilityAmount,

            Status:
                report.Status,

            OperativeAcc:
                report.OperativeAcc,

            State:
                report.State,

            LocalGov:
                report.LocalGov,

            PropertyDescription:
                report.PropertyDescription,

            Landsize:
                report.Landsize,

            ValuationDate:
                report.ValuationDate,

            OpenMarketValue:
                report.OpenMarketValue,

            ForceSalesValue:
                report.ForceSalesValue,

            InsuranceValue:
                report.InsuranceValue,

            SalesLocation:
                report.SalesLocation,

            SalesDate:
                report.SalesDate,

            ValuationReportName:
                report.ValuationReportName,

            ValuersName:
                report.ValuersName,

            DateAssigned:
                report.DateAssigned,

            ValuationReportBase64:
                report.ValuationReportBase64,

            ContentType:
                report.ContentType,

            ActualCoverageRatioPercent:
                actualPercent,

            RequiredCoverageRatioPercent:
                requiredPercent,

            CoverageAdequate:
                adequate
        );
    }
}