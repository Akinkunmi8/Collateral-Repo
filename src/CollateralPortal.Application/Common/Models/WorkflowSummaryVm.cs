namespace CollateralPortal.Application.Common.Models;

public sealed record WorkflowSummaryVm(
    int Total,
    int Pending,
    int WithinTat,
    int BeyondTat);
public sealed record CollateralDashboardSummaryVm(
    int Total,
    int Pending,
    int Archived,
    int Released,
    int WithinTat,
    int BeyondTat);