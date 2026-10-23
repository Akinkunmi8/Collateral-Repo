using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Application.Common.Models;

/// <summary>Shape returned by the (mocked) Newgen Loan Management System.</summary>
public record LoanDto(
    string LoanContractId,
    string CustomerId,
    string CustomerName,
    string AccountNumber,
    decimal ApprovedLimit,
    decimal ExposureLcy,
    DateTime StartDate,
    DateTime MaturityDate,
    string StateOfLocation,
    string ProductType);