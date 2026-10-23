using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Notifications;
using CollateralPortal.Domain.Entities;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.CreateCollateral;

public class CreateCollateralCommandHandler : IRequestHandler<CreateCollateralCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ILoanManagementClient _loanClient;
    private readonly ICoreBankingClient _coreBankingClient;
    private readonly ICreditRiskClient _creditRiskClient;
    private readonly IMessagePublisher _messagePublisher;

    public CreateCollateralCommandHandler(
        IApplicationDbContext db,
        ILoanManagementClient loanClient,
        ICoreBankingClient coreBankingClient,
        ICreditRiskClient creditRiskClient,
        IMessagePublisher messagePublisher)
    {
        _db = db;
        _loanClient = loanClient;
        _coreBankingClient = coreBankingClient;
        _creditRiskClient = creditRiskClient;
        _messagePublisher = messagePublisher;
    }

    public async Task<Guid> Handle(CreateCollateralCommand request, CancellationToken ct)
    {
        // UC-001: pull loan/customer/risk data from source systems (mocked) rather than re-keying it.
        // Whether this came from the automatic per-Loan-ID pull or the Collateral
        // Team keyed it in by hand, the same "Notification on collateral queue"
        // email goes out either way — this handler doesn't need to know which.
        var loan = await _loanClient.GetLoanAsync(request.LoanContractId, ct)
            ?? throw new NotFoundException("Loan", request.LoanContractId);

        var facility = await _coreBankingClient.GetFacilityAsync(request.LoanContractId, ct);
        var risk = await _creditRiskClient.GetRiskProfileAsync(loan.CustomerId, ct);

        var collateral = new Collateral
        {
            LoanContractId = loan.LoanContractId,
            CustomerId = loan.CustomerId,
            CustomerName = loan.CustomerName,
            AccountNumber = loan.AccountNumber,
            Type = request.Type,
            Region = request.Region,
            Description = request.Description,
            ApprovedLimit = loan.ApprovedLimit,
            FacilityType = request.FacilityType ?? loan.ProductType,
            Currency = request.Currency,
            CollateralAddress = request.CollateralAddress,
            CollateralOwner = request.CollateralOwner,
            BranchName = facility?.Branch,
            RelationshipManagerName = request.RelationshipManagerName,
            BranchManagerName = request.BranchManagerName
        };

        collateral.Submit();

        _db.Collaterals.Add(collateral);
        _db.AuditLogs.Add(new AuditLog
        {
            CollateralId = collateral.Id,
            Action = "Collateral data captured and submitted",
            PerformedBy = "system"
        });

        await _db.SaveChangesAsync(ct);

        var data = new CollateralQueueNotificationData(
            FacilityType: collateral.FacilityType ?? "N/A",
            CollateralType: collateral.Type.ToString(),
            CollateralDescription: collateral.Description,
            Currency: collateral.Currency,
            TotalCollateralValue: null, // populated once Valuation has run
            CollateralAddress: collateral.CollateralAddress,
            CollateralLocationState: collateral.Region.ToString(),
            AccountNumber: collateral.AccountNumber,
            ContractId: collateral.LoanContractId,
            LoanStartDate: loan.StartDate,
            LoanMaturityDate: loan.MaturityDate,
            PrincipalAmount: loan.ApprovedLimit,
            ExposureLcy: loan.ExposureLcy,
            ValuerName: null,
            ValuationStartDate: null,
            ValuationEndDate: null,
            LegalStatus: "Not yet started",
            StateOfLocation: loan.StateOfLocation,
            Region: collateral.Region.ToString(),
            InsuranceName: null,
            PremiumAmount: null,
            SumInsuredAmount: null,
            InsuranceStartDate: null,
            InsuranceEndDate: null,
            AssetType: collateral.Type.ToString(),
            CollateralOwner: collateral.CollateralOwner,
            RelationshipManagerName: collateral.RelationshipManagerName,
            BranchManagerName: collateral.BranchManagerName,
            NextRepaymentDate: facility?.NextRepaymentDate,
            LoanExposure: facility?.OutstandingBalance,
            BorrowerName: collateral.CustomerName,
            BranchName: collateral.BranchName ?? "N/A",
            CifId: collateral.CustomerId,
            RiskRating: risk?.RiskRating);

        var email = EmailTemplates.CollateralQueueNotification(collateral.CustomerName, data);

        await _messagePublisher.PublishAsync("collateral-notifications", new CollateralNotificationMessage(
            collateral.Id, "CollateralData", email.Subject, email.Body, CollateralNotificationMessage.StandardRecipients), ct);

        return collateral.Id;
    }
}
