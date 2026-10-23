using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Application.Collaterals.Queries.GetValuationReport;

public sealed record ValuationPreviewVm(
    Guid CollateralId,
    string LoanContractId,
    string AccountNumber,
    string CustomerName,
    decimal ApprovedLimit,
    string Currency,

    ValuationReportDto Report,

    decimal ActualCoverageRatioPercent,
    decimal RequiredCoverageRatioPercent,
    bool CoverageAdequate
);