namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralAssessmentReport;

public sealed record CollateralAssessmentReportVm(
    Guid CollateralId,
    string ReportReference,
    DateTime GeneratedAtUtc,

    ReportHeaderVm Header,
    AssessmentOverviewVm Overview,
    FacilityReportVm Facility,
    CollateralReportVm Collateral,
    ValuationAssessmentVm Valuation,
    CollateralControlPositionVm ControlPosition,
    IReadOnlyList<ReportDocumentVm> Documents,
    IReadOnlyList<ReportWorkflowHistoryVm> WorkflowHistory
);

public sealed record ReportHeaderVm(
    string LoanContractId,
    string CustomerId,
    string CustomerName,
    string AccountNumber,
    string CollateralType,
    string Region,
    string CurrentStage,
    string Status
);

public sealed record AssessmentOverviewVm(
    decimal ApprovedLimit,
    string Currency,
    decimal? OpenMarketValue,
    decimal? ForcedSaleValue,
    decimal? RequiredCoveragePercent,
    decimal? ActualCoveragePercent,
    string CoveragePosition,
    string CurrentPosition
);

public sealed record FacilityReportVm(
    string LoanContractId,
    string CustomerId,
    string CustomerName,
    string AccountNumber,
    string? FacilityType,
    decimal ApprovedLimit,
    string Currency,
    string? RelationshipManagerName,
    string? BranchManagerName,
    string? BranchName
);

public sealed record CollateralReportVm(
    string Type,
    string Region,
    string Description,
    string? Owner,
    string? Address
);

public sealed record ValuationAssessmentVm(
    bool Completed,
    string Status,
    DateTime? ValuationDate,
    string? ValuerName,
    decimal? OpenMarketValue,
    decimal? ForcedSaleValue,
    decimal? InsuranceValue,
    decimal? RequiredCoveragePercent,
    decimal? ActualCoveragePercent,
    string CoveragePosition,
    string? Decision,
    string? Remark
);

public sealed record CollateralControlPositionVm(
    LegalAssessmentVm Legal,
    InsuranceAssessmentVm Insurance,
    ReleaseAssessmentVm Release
);

public sealed record LegalAssessmentVm(
    bool Completed,
    string Status,
    string? RegistrationReference,
    string? LegalUnit,
    bool? CollateralValid,
    string? InvalidReason
);

public sealed record InsuranceAssessmentVm(
    bool Completed,
    string Status,
    string? PolicyNumber,
    string? Insurer,
    decimal? CoverageAmount,
    decimal? PremiumAmount,
    DateTime? PolicyStartDate,
    DateTime? PolicyExpiryDate,
    string? RenewalCadence,
    int? DaysToExpiry
);

public sealed record ReleaseAssessmentVm(
    bool Completed,
    string Status,
    string? RepaymentStatus,
    DateTime? ReleaseDate
);

public sealed record ReportDocumentVm(
    Guid Id,
    string DocumentType,
    string FileName,
    string? UploadedBy,
    DateTime UploadedAtUtc,
    long? FileSize
);

public sealed record ReportWorkflowHistoryVm(
    string Action,
    string PerformedBy,
    DateTime PerformedAtUtc,
    string? Details
);