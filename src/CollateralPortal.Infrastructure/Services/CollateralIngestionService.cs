using CollateralPortal.Application.Collaterals.Commands.CreateCollateral;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CollateralPortal.Infrastructure.BackgroundServices;

public sealed class CollateralIngestionService
    : ICollateralIngestionService
{
    private readonly ILoanManagementClient _loanManagementClient;
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CollateralIngestionService> _logger;

    public CollateralIngestionService(
        ILoanManagementClient loanManagementClient,
        IMediator mediator,
        ApplicationDbContext dbContext,
        ILogger<CollateralIngestionService> logger)
    {
        _loanManagementClient = loanManagementClient;
        _mediator = mediator;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> ImportAsync(
        CancellationToken cancellationToken = default)
    {
        var records =
            await _loanManagementClient.GetPendingCollateralsAsync(
                cancellationToken);

        if (records.Count == 0)
        {
            _logger.LogInformation(
                "No collateral records returned from source system.");

            return 0;
        }

        var importedCount = 0;

        foreach (var source in records)
        {
            try
            {
                var alreadyExists =
                    await _dbContext.Collaterals
                        .AsNoTracking()
                        .AnyAsync(
                            x => x.LoanContractId ==
                                 source.LoanContractId,
                            cancellationToken);

                if (alreadyExists)
                {
                    _logger.LogDebug(
                        "Collateral for {LoanContractId} already exists.",
                        source.LoanContractId);

                    continue;
                }

                var command =
                    new CreateCollateralCommand(
                        LoanContractId:
                            source.LoanContractId,

                        Type:
                            source.CollateralType,

                        Region:
                            source.Region,

                        Description:
                            source.CollateralDescription,

                        FacilityType:
                            source.FacilityType,

                        Currency:
                            source.Currency,

                        CollateralAddress:
                            source.CollateralAddress,

                        CollateralOwner:
                            source.CollateralOwner,

                        RelationshipManagerName:
                            source.RelationshipManagerName,

                        BranchManagerName:
                            source.BranchManagerName);

                var collateralId =
                    await _mediator.Send(
                        command,
                        cancellationToken);

                importedCount++;

                _logger.LogInformation(
                    "Collateral {CollateralId} imported " +
                    "for loan {LoanContractId}.",
                    collateralId,
                    source.LoanContractId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to import collateral for loan " +
                    "{LoanContractId}.",
                    source.LoanContractId);

                // Continue with remaining records.
            }
        }

        return importedCount;
    }
}