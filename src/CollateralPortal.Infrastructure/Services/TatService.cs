using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Domain.Enums;
using Microsoft.Extensions.Options;

namespace CollateralPortal.Infrastructure.Services;

public sealed class TatService : ITatService
{
    private readonly TatSettings _settings;

    public TatService(
        IOptions<TatSettings> options)
    {
        _settings = options.Value;
    }

    public int GetAllowedHours(WorkflowStage stage)
    {
        return stage switch
        {
            WorkflowStage.CollateralData
                => _settings.CollateralDataHours,

            WorkflowStage.Valuation
                => _settings.ValuationHours,

            WorkflowStage.Perfection
                => _settings.PerfectionHours,

            WorkflowStage.Insurance
                => _settings.InsuranceHours,

            // SystemHold, Release and Archived
            // do not have active TAT.
            _ => 0
        };
    }

    public TatStatus GetStatus(WorkflowStage stage,DateTime stageStartedAtUtc,DateTime? asOfUtc = null)
    {
        var allowedHours = GetAllowedHours(stage);

        // Stages without an active TAT should not
        // normally be passed into this method.
        if (allowedHours <= 0)
        {
            return TatStatus.WithinTat;
        }

        var now =
            asOfUtc ?? DateTime.UtcNow;

        var dueDate =
            stageStartedAtUtc.AddHours(
                allowedHours);

        return now <= dueDate
            ? TatStatus.WithinTat
            : TatStatus.BeyondTat;
    }

    public DateTime GetDueDateUtc(WorkflowStage stage,DateTime stageStartedAtUtc)
    {
        var allowedHours = GetAllowedHours(stage);

        if (allowedHours <= 0)
        {
            throw new InvalidOperationException(
                $"Workflow stage '{stage}' does not have an active TAT.");
        }

        return stageStartedAtUtc.AddHours(
            allowedHours);
    }

    public double GetElapsedHours(DateTime stageStartedAtUtc,DateTime? asOfUtc = null)
    {
        var now = asOfUtc ?? DateTime.UtcNow;

        return Math.Max(
            0,
            (now - stageStartedAtUtc)
                .TotalHours);
    }
}