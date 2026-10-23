using CollateralPortal.Application.Common.Interfaces;

namespace CollateralPortal.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
