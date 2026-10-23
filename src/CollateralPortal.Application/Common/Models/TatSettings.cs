namespace CollateralPortal.Application.Common.Models;

public sealed class TatSettings
{
    public const string SectionName = "TatSettings";

    public int CollateralDataHours { get; set; } = 24;
    public int ValuationHours { get; set; } = 72;
    public int PerfectionHours { get; set; } = 120;
    public int InsuranceHours { get; set; } = 48;
}