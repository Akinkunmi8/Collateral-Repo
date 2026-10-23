using CollateralPortal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralAssessmentReport;

public sealed class GetCollateralAssessmentReportQueryHandler
    : IRequestHandler<
        GetCollateralAssessmentReportQuery,
        CollateralAssessmentReportVm>
{
    private readonly IApplicationDbContext _db;

    public GetCollateralAssessmentReportQueryHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CollateralAssessmentReportVm> Handle(
        GetCollateralAssessmentReportQuery request,
        CancellationToken ct)
    {
        var collateral = await _db.Collaterals
            .AsNoTracking()
            .Include(x => x.Valuation)
            .Include(x => x.Perfection)
            .Include(x => x.Insurance)
            .Include(x => x.Release)
            .Include(x => x.Documents)
            .Include(x => x.AuditLogs)
            .FirstOrDefaultAsync(
                x => x.Id == request.CollateralId,
                ct);

        if (collateral is null)
        {
            throw new KeyNotFoundException(
                $"Collateral '{request.CollateralId}' was not found.");
        }

        var valuation = collateral.Valuation;
        var perfection = collateral.Perfection;
        var insurance = collateral.Insurance;
        var release = collateral.Release;

        var coveragePosition =
            GetCoveragePosition(
                valuation?.ActualCoverageRatioPercent,
                valuation?.RequiredCoverageRatioPercent);

        var reportReference =
            BuildReportReference(
                collateral.LoanContractId,
                collateral.Id);

        var valuationCompleted =
            valuation is not null;

        var legalCompleted =
            perfection is not null &&
            perfection.Stage.ToString() == "Completed";

        var insuranceCompleted =
            insurance is not null;

        var releaseCompleted =
            release is not null;

        var documents = collateral.Documents
            .OrderBy(x => x.DocumentType)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => new ReportDocumentVm(
                x.Id,
                x.DocumentType.ToString(),
                x.FileName,
                x.UploadedBy,
                x.CreatedAtUtc,
                x.FileSize))
            .ToList();

        var workflowHistory = collateral.AuditLogs
            .OrderBy(x => x.PerformedAtUtc)
            .Select(x => new ReportWorkflowHistoryVm(
                x.Action,
                x.PerformedBy,
                x.PerformedAtUtc,
                x.Details))
            .ToList();

        return new CollateralAssessmentReportVm(
            collateral.Id,
            reportReference,
            DateTime.UtcNow,

            new ReportHeaderVm(
                collateral.LoanContractId,
                collateral.CustomerId,
                collateral.CustomerName,
                collateral.AccountNumber,
                collateral.Type.ToString(),
                collateral.Region.ToString(),
                collateral.CurrentStage.ToString(),
                collateral.Status.ToString()),

            new AssessmentOverviewVm(
                collateral.ApprovedLimit,
                collateral.Currency,
                valuation?.OpenMarketValue,
                valuation?.ForcedSaleValue,
                valuation?.RequiredCoverageRatioPercent,
                valuation?.ActualCoverageRatioPercent,
                coveragePosition,
                GetCurrentPosition(
                    collateral.CurrentStage.ToString(),
                    collateral.Status.ToString())),

            new FacilityReportVm(
                collateral.LoanContractId,
                collateral.CustomerId,
                collateral.CustomerName,
                collateral.AccountNumber,
                collateral.FacilityType,
                collateral.ApprovedLimit,
                collateral.Currency,
                collateral.RelationshipManagerName,
                collateral.BranchManagerName,
                collateral.BranchName),

            new CollateralReportVm(
                collateral.Type.ToString(),
                collateral.Region.ToString(),
                collateral.Description,
                collateral.CollateralOwner,
                collateral.CollateralAddress),

            new ValuationAssessmentVm(
                valuationCompleted,
                valuationCompleted
                    ? "Completed"
                    : GetSectionStatus(
                        collateral.CurrentStage.ToString(),
                        "Valuation"),
                valuation?.ValuationDate,
                valuation?.ValuersName,
                valuation?.OpenMarketValue,
                valuation?.ForcedSaleValue,
                valuation?.InsuranceValue,
                valuation?.RequiredCoverageRatioPercent,
                valuation?.ActualCoverageRatioPercent,
                coveragePosition,
                valuation?.Decision.ToString(),
                valuation?.Remark),

            new CollateralControlPositionVm(

                new LegalAssessmentVm(
                    legalCompleted,
                    GetLegalStatus(
                        collateral.CurrentStage.ToString(),
                        perfection?.Stage.ToString()),
                    perfection?.RegistrationReference,
                    perfection?.LegalUnit,
                    perfection?.CollateralValid,
                    perfection?.InvalidReason),

                new InsuranceAssessmentVm(
                    insuranceCompleted,
                    GetInsuranceStatus(
                        collateral.CurrentStage.ToString(),
                        insurance is not null),
                    insurance?.PolicyNumber,
                    insurance?.Insurer,
                    insurance?.CoverageAmount,
                    insurance?.PremiumAmount,
                    insurance?.PolicyStartDate,
                    insurance?.PolicyExpiryDate,
                    insurance?.RenewalCadence.ToString(),
                    CalculateDaysToExpiry(
                        insurance?.PolicyExpiryDate)),

                new ReleaseAssessmentVm(
                    releaseCompleted,
                    GetReleaseStatus(
                        collateral.CurrentStage.ToString(),
                        release is not null),
                    release is null
                        ? null
                        : release.LoanFullyRepaid
                            ? "Fully Repaid"
                            : "Outstanding",
                    release?.ReleaseDate)
            ),
            documents,
            workflowHistory
        );

    }

    private static string GetCoveragePosition(
        decimal? actual,
        decimal? required)
    {
        if (!actual.HasValue ||
            !required.HasValue)
        {
            return "Not Assessed";
        }

        return actual.Value >= required.Value
            ? "Adequate"
            : "Below Requirement";
    }

    private static string GetCurrentPosition(
        string stage,
        string status)
    {
        return $"{FormatEnum(stage)} - {FormatEnum(status)}";
    }

    private static string GetSectionStatus(
        string currentStage,
        string targetStage)
    {
        if (string.Equals(
            currentStage,
            targetStage,
            StringComparison.OrdinalIgnoreCase))
        {
            return "In Progress";
        }

        return "Not Yet Commenced";
    }

    private static string GetLegalStatus(
        string currentStage,
        string? perfectionStage)
    {
        if (string.Equals(
            perfectionStage,
            "Completed",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Completed";
        }

        if (string.Equals(
            currentStage,
            "Perfection",
            StringComparison.OrdinalIgnoreCase))
        {
            return "In Progress";
        }

        return "Not Yet Commenced";
    }

    private static string GetInsuranceStatus(
        string currentStage,
        bool hasInsurance)
    {
        if (hasInsurance)
        {
            return "Completed";
        }

        if (string.Equals(
            currentStage,
            "Insurance",
            StringComparison.OrdinalIgnoreCase))
        {
            return "In Progress";
        }

        return "Not Yet Commenced";
    }

    private static string GetReleaseStatus(
        string currentStage,
        bool hasRelease)
    {
        if (hasRelease)
        {
            return "Released";
        }

        if (string.Equals(
                currentStage,
                "SystemHold",
                StringComparison.OrdinalIgnoreCase) ||
            string.Equals(
                currentStage,
                "Release",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Awaiting Release";
        }

        return "Not Eligible";
    }

    private static int? CalculateDaysToExpiry(
        DateTime? expiryDate)
    {
        if (!expiryDate.HasValue)
        {
            return null;
        }

        return (expiryDate.Value.Date -
                DateTime.UtcNow.Date).Days;
    }

    private static string BuildReportReference(
        string loanContractId,
        Guid collateralId)
    {
        var shortId = collateralId
            .ToString("N")[..8]
            .ToUpperInvariant();

        return $"CMS/CAR/{DateTime.UtcNow:yyyy}/{loanContractId}/{shortId}";
    }

    private static string FormatEnum(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return System.Text.RegularExpressions.Regex.Replace(
            value,
            "([a-z])([A-Z])",
            "$1 $2");
    }

}