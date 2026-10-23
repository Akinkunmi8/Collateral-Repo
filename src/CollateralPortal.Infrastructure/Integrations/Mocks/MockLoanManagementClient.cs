using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Infrastructure.Integrations.Mocks;

/// <summary>
/// Development implementation of the Loan Management integration.
///
/// Simulates data returned by Newgen Smart Process until the real
/// third-party integration is available.
///
/// LoanFixtures represents loan/customer/facility information.
///
/// CollateralFixtures represents collateral information waiting
/// to be automatically pulled into the Collateral Portal.
/// </summary>
public sealed class MockLoanManagementClient : ILoanManagementClient
{
    #region Loan Fixtures

    private static readonly IReadOnlyDictionary<string, LoanDto>
        LoanFixtures =
            new Dictionary<string, LoanDto>
            {
                ["LN-2026-00123"] = new LoanDto(
                    LoanContractId: "LN-2026-00123",
                    CustomerId: "CIF-000456",
                    CustomerName: "Adaeze Okafor Enterprises",
                    AccountNumber: "1000000000",
                    ApprovedLimit: 50_000_000m,
                    ExposureLcy: 42_500_000m,
                    StartDate: DateTime.UtcNow.AddMonths(-6),
                    MaturityDate: DateTime.UtcNow.AddYears(2),
                    StateOfLocation: "Lagos",
                    ProductType: "Term Loan"
                ),

                ["LN-2026-00456"] = new LoanDto(
                    LoanContractId: "LN-2026-00456",
                    CustomerId: "CIF-000789",
                    CustomerName: "Bello & Sons Trading Co.",
                    AccountNumber: "2000000111",
                    ApprovedLimit: 15_000_000m,
                    ExposureLcy: 15_000_000m,
                    StartDate: DateTime.UtcNow.AddMonths(-18),
                    MaturityDate: DateTime.UtcNow.AddMonths(-1),
                    StateOfLocation: "Kano",
                    ProductType: "Overdraft"
                ),

                ["LN-2026-00789"] = new LoanDto(
                    LoanContractId: "LN-2026-00789",
                    CustomerId: "CIF-001011",
                    CustomerName: "Chukwuma Logistics Ltd",
                    AccountNumber: "3000000222",
                    ApprovedLimit: 120_000_000m,
                    ExposureLcy: 95_000_000m,
                    StartDate: DateTime.UtcNow.AddYears(-1),
                    MaturityDate: DateTime.UtcNow.AddYears(3),
                    StateOfLocation: "Abuja",
                    ProductType: "Asset Finance"
                )
            };

    #endregion

    #region Collateral Fixtures

    private static readonly IReadOnlyDictionary<string, CollateralSourceDto>
        CollateralFixtures =
            new Dictionary<string, CollateralSourceDto>
            {
                ["LN-2026-00123"] = new CollateralSourceDto(
                    LoanContractId: "LN-2026-00123",

                    CollateralType:
                        CollateralType.RealEstate,

                    Region:
                        Region.Lagos,

                    CollateralDescription:
                        "Commercial property pledged as security for term loan facility",

                    FacilityType:
                        "Term Loan",

                    Currency:
                        "NGN",

                    CollateralAddress:
                        "Ikeja, Lagos State",

                    CollateralOwner:
                        "Adaeze Okafor Enterprises",

                    RelationshipManagerName:
                        "Chinedu Eze",

                    BranchManagerName:
                        "Funke Adeyemi"
                ),

                ["LN-2026-00456"] = new CollateralSourceDto(
                    LoanContractId: "LN-2026-00456",

                    CollateralType:
                        CollateralType.RealEstate,

                    Region:
                        Region.OtherRegion,

                    CollateralDescription:
                        "Commercial warehouse pledged as security for overdraft facility",

                    FacilityType:
                        "Overdraft",

                    Currency:
                        "NGN",

                    CollateralAddress:
                        "Bompai Industrial Area, Kano State",

                    CollateralOwner:
                        "Bello & Sons Trading Co.",

                    RelationshipManagerName:
                        "Ibrahim Musa",

                    BranchManagerName:
                        "Amina Abdullahi"
                ),

                ["LN-2026-00789"] = new CollateralSourceDto(
                    LoanContractId: "LN-2026-00789",

                    CollateralType:
                        CollateralType.MovableAsset,

                    Region:
                        Region.Abuja,

                    CollateralDescription:
                        "Commercial logistics vehicles pledged as security for asset finance facility",

                    FacilityType:
                        "Asset Finance",

                    Currency:
                        "NGN",

                    CollateralAddress:
                        "Idu Industrial Area, Abuja",

                    CollateralOwner:
                        "Chukwuma Logistics Ltd",

                    RelationshipManagerName:
                        "Emeka Nwosu",

                    BranchManagerName:
                        "Grace Okonkwo"
                )
            };

    #endregion

    #region Loan Methods

    /// <summary>
    /// Returns loan/customer/facility information for a specific
    /// loan contract.
    ///
    /// Used by CreateCollateralCommandHandler when creating the
    /// actual collateral record.
    /// </summary>
    public Task<LoanDto?> GetLoanAsync(
        string loanContractId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LoanFixtures.TryGetValue(
            loanContractId,
            out var loan);

        return Task.FromResult(loan);
    }

    /// <summary>
    /// Returns all available loan records.
    ///
    /// Retained for functionality that needs the loan feed directly.
    /// </summary>
    public Task<IReadOnlyList<LoanDto>> GetPendingLoansAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<LoanDto> loans =
            LoanFixtures.Values.ToList();

        return Task.FromResult(loans);
    }

    #endregion

    #region Collateral Methods

    /// <summary>
    /// Returns complete collateral creation information for a
    /// specific loan contract.
    /// </summary>
    public Task<CollateralSourceDto?> GetCollateralAsync(
        string loanContractId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CollateralFixtures.TryGetValue(
            loanContractId,
            out var collateral);

        return Task.FromResult(collateral);
    }

    /// <summary>
    /// Returns collateral records available for automatic import.
    ///
    /// This mock always returns the fixture records. The ingestion
    /// service is responsible for checking the Collaterals table and
    /// preventing duplicate imports.
    /// </summary>
    public Task<IReadOnlyList<CollateralSourceDto>>
        GetPendingCollateralsAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<CollateralSourceDto> collaterals =
            CollateralFixtures.Values.ToList();

        return Task.FromResult(collaterals);
    }

    #endregion
}