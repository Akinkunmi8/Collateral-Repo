using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetAuditLogs;

public record GetAuditLogsQuery(int Take = 50) : IRequest<List<AuditLogVm>>;

public record AuditLogVm(Guid CollateralId, string Action, string PerformedBy, DateTime PerformedAtUtc, string? Details);

public sealed record ExportAuditLogsQuery: IRequest<IReadOnlyList<AuditLogVm>>;
