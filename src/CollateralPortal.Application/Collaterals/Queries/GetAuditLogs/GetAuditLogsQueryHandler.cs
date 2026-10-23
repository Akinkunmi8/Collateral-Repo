using CollateralPortal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler: IRequestHandler<GetAuditLogsQuery, List<AuditLogVm>>
{
    private readonly IApplicationDbContext _db;

    public GetAuditLogsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<AuditLogVm>> Handle(GetAuditLogsQuery request,CancellationToken ct)
    {
        return await _db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.PerformedAtUtc)
            .Take(request.Take)
            .Select(x => new AuditLogVm(
                x.CollateralId,
                x.Action,
                x.PerformedBy,
                x.PerformedAtUtc,
                x.Details))
            .ToListAsync(ct);
    }
}