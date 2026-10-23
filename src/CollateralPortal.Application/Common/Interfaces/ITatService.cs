using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Application.Common.Interfaces;

public interface ITatService
{
    int GetAllowedHours(WorkflowStage stage);

    TatStatus GetStatus(
        WorkflowStage stage,
        DateTime stageStartedAtUtc,
        DateTime? asOfUtc = null);

    DateTime GetDueDateUtc(
        WorkflowStage stage,
        DateTime stageStartedAtUtc);

    double GetElapsedHours(
        DateTime stageStartedAtUtc,
        DateTime? asOfUtc = null);
}