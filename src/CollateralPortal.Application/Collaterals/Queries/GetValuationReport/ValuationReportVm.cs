namespace CollateralPortal.Application.Collaterals.Queries.GetValuationReport;

public sealed record ValuationReportVm(
    Guid CollateralId,

    string LoanContractId,
    string AccountNumber,
    string CustomerName,
    decimal ApprovedLimit,
    string Currency,

    string? FacilityRef,
    decimal FacilityAmount,
    string? Status,
    string? OperativeAcc,
    string? State,
    string? LocalGov,
    string? PropertyDescription,
    string? Landsize,
    DateTime? ValuationDate,
    decimal? OpenMarketValue,
    decimal? ForceSalesValue,
    decimal? InsuranceValue,
    string? SalesLocation,
    DateTime? SalesDate,
    string? ValuationReportName,
    string? ValuersName,
    DateTime? DateAssigned,

    string? ValuationReportBase64,
    string? ContentType,

    decimal ActualCoverageRatioPercent,
    decimal RequiredCoverageRatioPercent,
    bool CoverageAdequate
);