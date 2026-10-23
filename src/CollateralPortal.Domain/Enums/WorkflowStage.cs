namespace CollateralPortal.Domain.Enums;

// Mirrors the UC-001..UC-006 stages from the BRD process flow
public enum WorkflowStage
{
    CollateralData = 1,
    Valuation = 2,
    Perfection = 3,
    Insurance = 4,
    SystemHold = 5,
    Release = 6,
    Archived = 7
}
