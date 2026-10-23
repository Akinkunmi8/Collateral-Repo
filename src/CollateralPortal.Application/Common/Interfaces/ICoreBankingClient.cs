using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Application.Common.Interfaces;

/// <summary>Port to Vision (core banking). Mocked for now.</summary>
public interface ICoreBankingClient
{
    Task<CoreBankingFacilityDto?> GetFacilityAsync(string loanContractId, CancellationToken ct = default);
}
