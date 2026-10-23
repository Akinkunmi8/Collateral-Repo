using CollateralPortal.Api.Authorization;
using CollateralPortal.Application.Collaterals.Queries.GetArchivedCollaterals;
using CollateralPortal.Application.Collaterals.Queries.GetAuditLogs;
using CollateralPortal.Application.Collaterals.Queries.GetCollateralRegister;
using CollateralPortal.Application.Collaterals.Queries.GetCollateralWorkItem;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollateralPortal.Api.Controllers;

/// <summary>
/// Per the workstep UI spec: "Display the [X] queue on the user's dashboard
/// upon successful login for selection... where the user selects the
/// workitem, the system shall display [tabs]." Each module has its own
/// queue + workitem-detail pair, gated by that module's own Read policy —
/// this is what lets Legal/Insurance Team see their own queue without
/// needing CollateralData access.
/// </summary>
[ApiController]
[Route("api/dashboards")]
[Authorize]
public class DashboardsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICsvExportService _csvExportService;
    public DashboardsController(IMediator mediator,ICsvExportService csvExportService)
    {
        _mediator = mediator;
        _csvExportService = csvExportService;
    }

    // ---- Collateral Data ----
    [HttpGet("collateral-data/queue")]
    [RequireDashboard(DashboardModule.CollateralData, DashboardAccessLevel.Read)]
    public async Task<ActionResult<List<CollateralRegisterItemVm>>> CollateralDataQueue(CancellationToken ct)
        => Ok(await _mediator.Send(new GetCollateralRegisterQuery("CollateralData"), ct));

    [HttpGet("collateral-data/{id:guid}")]
    [RequireDashboard(DashboardModule.CollateralData, DashboardAccessLevel.Read)]
    public async Task<ActionResult<CollateralWorkItemVm>> CollateralDataWorkItem(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetCollateralWorkItemQuery(id), ct));

    // ---- Valuation ----
    [HttpGet("valuation/queue")]
    [RequireDashboard(DashboardModule.Valuation, DashboardAccessLevel.Read)]
    public async Task<ActionResult<List<CollateralRegisterItemVm>>> ValuationQueue(CancellationToken ct)
        => Ok(await _mediator.Send(new GetCollateralRegisterQuery("Valuation"), ct));

    [HttpGet("valuation/{id:guid}")]
    [RequireDashboard(DashboardModule.Valuation, DashboardAccessLevel.Read)]
    public async Task<ActionResult<CollateralWorkItemVm>> ValuationWorkItem(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetCollateralWorkItemQuery(id), ct));

    // ---- Legal (Perfection) ----
    [HttpGet("legal/queue")]
    [RequireDashboard(DashboardModule.Legal,DashboardAccessLevel.Read)]
    public async Task<ActionResult<PagedResult<CollateralRegisterItemVm>>>LegalQueue(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] string? tat,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetCollateralRegisterQuery(
                Search: search,
                StageFilter:
                    WorkflowStage.Perfection.ToString(),
                TypeFilter: type,
                StatusFilter:
                    CollateralStatus.InPerfection.ToString(),
                TatFilter: tat,
                PageNumber: pageNumber,
                PageSize: pageSize),
            ct);

        return Ok(result);
    }

    [HttpGet("legal/{id:guid}")]
    [RequireDashboard(DashboardModule.Legal, DashboardAccessLevel.Read)]
    public async Task<ActionResult<CollateralWorkItemVm>> LegalWorkItem(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetCollateralWorkItemQuery(id), ct));

    // ---- Insurance ----
    // ---- Insurance ----

    [HttpGet("insurance/queue")]
    [RequireDashboard(DashboardModule.Insurance,DashboardAccessLevel.Read)]
    public async Task<ActionResult<PagedResult<CollateralRegisterItemVm>>>InsuranceQueue(
            [FromQuery] string? search,
            [FromQuery] string? type,
            [FromQuery] string? tat,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetCollateralRegisterQuery(
                Search: search,
                StageFilter:
                    WorkflowStage.Insurance.ToString(),
                TypeFilter: type,
                StatusFilter:
                    CollateralStatus.InInsurance.ToString(),
                TatFilter: tat,
                PageNumber: pageNumber,
                PageSize: pageSize),
            ct);

        return Ok(result);
    }

    [HttpGet("insurance/{id:guid}")]
    [RequireDashboard(DashboardModule.Insurance,DashboardAccessLevel.Read)]
    public async Task<ActionResult<CollateralWorkItemVm>>InsuranceWorkItem(Guid id,CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetCollateralWorkItemQuery(id),
            ct);

        return Ok(result);
    }

    // ---- Release (System Hold → Release) ----
    // ---- Release (System Hold → Release) ----

    [HttpGet("release/queue")]
    [RequireDashboard(DashboardModule.Release,DashboardAccessLevel.Read)]
    public async Task<ActionResult<PagedResult<CollateralRegisterItemVm>>>ReleaseQueue(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetCollateralRegisterQuery(
                Search: search,
                StageFilter: WorkflowStage.SystemHold.ToString(),
                TypeFilter: type,
                StatusFilter: CollateralStatus.OnHold.ToString(),
                TatFilter: null,
                PageNumber: pageNumber,
                PageSize: pageSize),
            ct);

        return Ok(result);
    }

    [HttpGet("release/{id:guid}")]
    [RequireDashboard(DashboardModule.Release,DashboardAccessLevel.Read)]
    public async Task<ActionResult<CollateralWorkItemVm>> ReleaseWorkItem(Guid id,CancellationToken ct)
    {
        return Ok(
            await _mediator.Send(
                new GetCollateralWorkItemQuery(id),
                ct));
    }
    // Archive
    [HttpGet("release/archive")]
    [RequireDashboard(DashboardModule.Release,DashboardAccessLevel.Read)]
    public async Task<ActionResult<PagedResult<ArchivedCollateralVm>>>ArchivedCollaterals(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetArchivedCollateralsQuery(
                Search: search,
                PageNumber: pageNumber,
                PageSize: pageSize),
            ct);

        return Ok(result);
    }
    // ---- Audit ----
    [HttpGet("audit")]
    [RequireDashboard(DashboardModule.Audit, DashboardAccessLevel.Read)]
    public async Task<ActionResult<List<AuditLogVm>>> Audit(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAuditLogsQuery(), ct));

    [HttpGet("audit/export")]
    [RequireDashboard(DashboardModule.Audit,DashboardAccessLevel.Read)]
    public async Task<IActionResult> ExportAudit(CancellationToken ct)
    {
        var records = await _mediator.Send(new ExportAuditLogsQuery(),ct);

        var columns =
            new ICsvExportService.CsvColumn<AuditLogVm>[]
            {
            new(
                "Collateral ID",
                x => x.CollateralId),

            new(
                "Action",
                x => x.Action),

            new(
                "Performed By",
                x => x.PerformedBy),

            new(
                "Date/Time UTC",
                x => x.PerformedAtUtc),

            new(
                "Details",
                x => x.Details)
            };

        var csv = _csvExportService.Generate(
            records,
            columns);

        var fileName =
            $"audit-trail-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(
            csv,
            "text/csv; charset=utf-8",
            fileName);
    }

    // ---- Admin ----
    [HttpGet("admin")]
    [RequireDashboard(DashboardModule.Admin, DashboardAccessLevel.Read)]
    public async Task<ActionResult<List<CollateralRegisterItemVm>>> Admin(CancellationToken ct)
        => Ok(await _mediator.Send(new GetCollateralRegisterQuery(), ct));
}
