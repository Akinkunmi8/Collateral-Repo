using CollateralPortal.Domain.Common;
using CollateralPortal.Domain.Enums;
using CollateralPortal.Domain.Events;

namespace CollateralPortal.Domain.Entities;

public class Collateral : BaseAuditableEntity
{
    public string LoanContractId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public string CustomerName { get; set; } = default!;

    public string AccountNumber { get; set; } = default!;

    public CollateralType Type { get; set; }
    public Region Region { get; set; }
    public string Description { get; set; } = default!;

    public decimal ApprovedLimit { get; set; }
    public decimal? RequiredCoverageRatioPercent { get; set; }

    public string? FacilityType { get; set; }
    public string Currency { get; set; } = "NGN";
    public string? CollateralAddress { get; set; }
    public string? CollateralOwner { get; set; }
    public string? BranchName { get; set; }
    public string? RelationshipManagerName { get; set; }
    public string? BranchManagerName { get; set; }

    public WorkflowStage CurrentStage { get; private set; }
        = WorkflowStage.CollateralData;

    public CollateralStatus Status { get; private set; }
        = CollateralStatus.Draft;

    /// <summary>
    /// UTC timestamp when the collateral entered its current workflow stage.
    /// Used as the starting point for TAT calculations.
    /// </summary>
    public DateTime StageStartedAtUtc { get; private set; }

    public CollateralValuation? Valuation { get; set; }
    public CollateralPerfection? Perfection { get; set; }
    public CollateralInsurance? Insurance { get; set; }
    public CollateralRelease? Release { get; set; }

    public ICollection<CollateralDocument> Documents { get; set; }
        = new List<CollateralDocument>();

    public ICollection<AuditLog> AuditLogs { get; set; }
        = new List<AuditLog>();

    public void Submit()
    {
        TransitionTo(
            WorkflowStage.CollateralData,
            CollateralStatus.PendingApproval);

        AddDomainEvent(
            new CollateralSubmittedEvent(
                Id,
                WorkflowStage.CollateralData));
    }

    public void MoveToValuation()
    {
        TransitionTo(
            WorkflowStage.Valuation,
            CollateralStatus.InValuation);
    }

    public void RequestAdditionalCollateral()
    {
        TransitionTo(
            WorkflowStage.CollateralData,
            CollateralStatus.AdditionalCollateralRequested);

        AddDomainEvent(
            new CoverageShortfallEvent(Id));
    }

    public void MoveToPerfection()
    {
        TransitionTo(
            WorkflowStage.Perfection,
            CollateralStatus.InPerfection);
    }

    public void MoveToInsurance()
    {
        TransitionTo(
            WorkflowStage.Insurance,
            CollateralStatus.InInsurance);
    }

    public void PlaceOnHold()
    {
        TransitionTo(
            WorkflowStage.SystemHold,
            CollateralStatus.OnHold);
    }

    public void MarkReleased(string releasedBy)
    {
        TransitionTo(
            WorkflowStage.Release,
            CollateralStatus.Released);

        AddDomainEvent(
            new CollateralReleasedEvent(
                Id,
                releasedBy));
    }

    public void Archive()
    {
        TransitionTo(
            WorkflowStage.Archived,
            CollateralStatus.Archived);
    }

    private void TransitionTo(
        WorkflowStage stage,
        CollateralStatus status)
    {
        CurrentStage = stage;
        Status = status;
        StageStartedAtUtc = DateTime.UtcNow;
    }
    public void ResubmitForVerification()
    {
        if (CurrentStage != WorkflowStage.CollateralData ||
            Status != CollateralStatus.AdditionalCollateralRequested)
        {
            throw new InvalidOperationException(
                "Only collateral awaiting additional information can be resubmitted.");
        }

        TransitionTo(
            WorkflowStage.CollateralData,
            CollateralStatus.PendingApproval);
    }

    public void RequestAdditionalValuationInformation()
    {
        if (CurrentStage != WorkflowStage.Valuation ||
            Status != CollateralStatus.InValuation)
        {
            throw new InvalidOperationException(
                "Additional valuation information can only be requested for collateral currently in Valuation.");
        }

        TransitionTo(
            WorkflowStage.Valuation,
            CollateralStatus.AdditionalValuationInformationRequested);
    }
    public void ResubmitForValuationVerification()
    {
        if (CurrentStage != WorkflowStage.Valuation ||
            Status != CollateralStatus.AdditionalValuationInformationRequested)
        {
            throw new InvalidOperationException(
                "Only collateral awaiting additional valuation information can be resubmitted.");
        }

        TransitionTo(
            WorkflowStage.Valuation,
            CollateralStatus.InValuation);
    }
}