using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Application.Common.Interfaces;

public interface ILoanManagementClient
{
    Task<LoanDto?> GetLoanAsync(
        string loanContractId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoanDto>> GetPendingLoansAsync(
        CancellationToken cancellationToken = default);

    Task<CollateralSourceDto?> GetCollateralAsync(
        string loanContractId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CollateralSourceDto>>
        GetPendingCollateralsAsync(
            CancellationToken cancellationToken = default);
}