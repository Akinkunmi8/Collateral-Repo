using CollateralPortal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetAuditLogs;

public sealed class ExportAuditLogsQueryHandler
    : IRequestHandler<
        ExportAuditLogsQuery,
        IReadOnlyList<AuditLogVm>>
{
    private readonly IApplicationDbContext _db;

    public ExportAuditLogsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AuditLogVm>> Handle(ExportAuditLogsQuery request,CancellationToken ct)
    {
        return await _db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.PerformedAtUtc)
            .Select(x => new AuditLogVm(
                x.CollateralId,
                x.Action,
                x.PerformedBy,
                x.PerformedAtUtc,
                x.Details))
            .ToListAsync(ct);
    }
}