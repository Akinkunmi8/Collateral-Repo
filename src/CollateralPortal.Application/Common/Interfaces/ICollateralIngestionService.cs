namespace CollateralPortal.Application.Common.Interfaces;

public interface ICollateralIngestionService
{
    Task<int> ImportAsync(CancellationToken cancellationToken = default);
}