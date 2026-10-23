namespace CollateralPortal.Application.Common.Models;

/// <summary>
/// Mirrors the real Valuation Portal's CompletedValuationReportDto exactly
/// (GET /api/ValuationReport/completed-by-account/{accountNumber}), so the
/// adapter can deserialize directly into this type with no field mapping.
/// </summary>
public record ValuationReportDto(
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
    string? ContentType);
