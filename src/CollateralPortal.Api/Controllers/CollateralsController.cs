using CollateralPortal.Api.Authorization;
using CollateralPortal.Application.Collaterals.Commands.CreateCollateral;
using CollateralPortal.Application.Collaterals.Commands.ReleaseCollateral;
using CollateralPortal.Application.Collaterals.Commands.ResubmitCollateral;
using CollateralPortal.Application.Collaterals.Commands.ResubmitValuation;
using CollateralPortal.Application.Collaterals.Commands.SubmitInsurance;
using CollateralPortal.Application.Collaterals.Commands.SubmitPerfection;
using CollateralPortal.Application.Collaterals.Commands.SubmitValuation;
using CollateralPortal.Application.Collaterals.Commands.VerifyCollateral;
using CollateralPortal.Application.Collaterals.Queries.GetCollateralAssessmentReport;
using CollateralPortal.Application.Collaterals.Queries.GetCollateralById;
using CollateralPortal.Application.Collaterals.Queries.GetCollateralRegister;
using CollateralPortal.Application.Collaterals.Queries.GetCollateralSummary;
using CollateralPortal.Application.Collaterals.Queries.GetReleaseRepaymentStatus;
using CollateralPortal.Application.Collaterals.Queries.GetValuationQueue;
using CollateralPortal.Application.Collaterals.Queries.GetValuationReport;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollateralPortal.Api.Controllers;

[ApiController]
[Route("api/collaterals")]
[Authorize]
public class CollateralsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICollateralReportDocumentService _reportDocumentService;
    public CollateralsController(IMediator mediator, ICollateralReportDocumentService reportDocumentService)
    {
        _mediator = mediator;
        _reportDocumentService = reportDocumentService;
    }
    /// <summary>UC-001 — capture collateral data (pulls loan/customer/risk data from mocked source systems).</summary>
    [HttpPost]
    [RequireDashboard(DashboardModule.CollateralData, DashboardAccessLevel.Write)]
    public async Task<ActionResult<Guid>> Create(CreateCollateralCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>Collateral detail — same access rule as the register (CollateralData read).</summary>
    [HttpGet("{id:guid}")]
    [RequireDashboard(DashboardModule.CollateralData, DashboardAccessLevel.Read)]
    public async Task<ActionResult<CollateralDetailVm>> GetById(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetCollateralByIdQuery(id), ct));

    /// <summary>
    /// The collateral register — the single source-of-truth view. Gated on
    /// CollateralData read access, so Legal/Insurance Team (who only have
    /// their own dashboard) cannot browse the full register.
    /// </summary>
    [HttpGet]
    [RequireDashboard(
    DashboardModule.CollateralData,
    DashboardAccessLevel.Read)]
    public async Task<ActionResult<PagedResult<CollateralRegisterItemVm>>> GetRegister(
    [FromQuery] string? search,
    [FromQuery] string? stageFilter,
    [FromQuery] string? typeFilter,
    [FromQuery] string? statusFilter,
    [FromQuery] string? tatFilter,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
    {
        var query = new GetCollateralRegisterQuery(
            Search: search,
            StageFilter: stageFilter,
            TypeFilter: typeFilter,
            StatusFilter: statusFilter,
            TatFilter: tatFilter,
            PageNumber: pageNumber,
            PageSize: pageSize);

        var result = await _mediator.Send(
            query,
            ct);

        return Ok(result);
    }
    /// <summary>
    /// UC-002 — verifies collateral data before the record
    /// enters the valuation workflow.
    /// </summary>
    [HttpPost("{id:guid}/verify")]
    [RequireDashboard(
        DashboardModule.CollateralData,
        DashboardAccessLevel.Write)]
    public async Task<IActionResult> VerifyCollateral(Guid id, VerifyCollateralRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new VerifyCollateralCommand(
                id,
                request.Decision,
                request.Remark),
            ct);

        return NoContent();
    }
    [HttpPost("{id:guid}/resubmit")]
    [RequireDashboard(DashboardModule.CollateralData,DashboardAccessLevel.Write)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResubmitCollateral(
    Guid id,
    ResubmitCollateralRequest request,
    CancellationToken ct)
    {
        await _mediator.Send(
            new ResubmitCollateralCommand(
                id,
                request.Response),
            ct);

        return NoContent();
    }

    /// <summary>
    /// UC-002 — processes the Valuation Team's decision.
    ///
    /// Automatic path:
    /// The backend retrieves the latest completed valuation report
    /// from the Valuation Portal using the collateral's AccountNumber.
    ///
    /// Manual path:
    /// Manual valuation values and an attached report are accepted
    /// when manual entry is explicitly used.
    ///
    /// A valuation is persisted only when the decision is Submit.
    /// Requesting additional valuation information only updates the
    /// workflow/audit state.
    /// </summary>
    [HttpPost("{id:guid}/valuation")]
    [RequireDashboard(DashboardModule.Valuation,DashboardAccessLevel.Write)]
    public async Task<ActionResult<SubmitValuationResult>> SubmitValuation(
        Guid id,
        SubmitValuationRequest request,
        CancellationToken ct)
    {
        var command = new SubmitValuationCommand(
            CollateralId: id,

            ManualEntry: request.ManualEntry,
            ManualOpenMarketValue: request.ManualOpenMarketValue,
            ManualForcedSaleValue: request.ManualForcedSaleValue,
            ManualValuersName: request.ManualValuersName,
            ManualValuationDate: request.ManualValuationDate,

            ReportBase64: request.ReportBase64 ?? string.Empty,
            ReportContentType: request.ReportContentType ?? string.Empty,

            Decision: request.Decision,
            Remark: request.Remark
        );

        var result = await _mediator.Send(
            command,
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Returns collateral currently awaiting processing by the
    /// Valuation Team.
    /// </summary>
    [HttpGet("valuation")]
public async Task<ActionResult<PagedResult<ValuationQueueItemVm>>> GetValuationQueue(
    [FromQuery] string? search,
    [FromQuery] string? status,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken cancellationToken = default)
{
    var result = await _mediator.Send(
        new GetValuationQueueQuery(
            search,
            status,
            pageNumber,
            pageSize),
        cancellationToken);

    return Ok(result);
}
    /// <summary>
    /// Retrieves the latest completed valuation report from the
    /// Valuation Portal using the collateral's AccountNumber.
    ///
    /// This endpoint is read-only. Retrieving the report does not
    /// create a CollateralValuation record.
    /// </summary>
    [HttpGet("{id:guid}/valuation")]
    [RequireDashboard(DashboardModule.Valuation,DashboardAccessLevel.Read)]
    public async Task<ActionResult<ValuationPreviewVm>> GetValuation(
        Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetValuationReportQuery(id),
            ct);

        return Ok(result);
    }

    [HttpPost("{id:guid}/valuation/resubmit")]
    [RequireDashboard(DashboardModule.Valuation,DashboardAccessLevel.Write)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResubmitValuation(Guid id, ResubmitValuationRequest request,
    CancellationToken ct)
    {
        await _mediator.Send(
            new ResubmitValuationCommand(
                id,
                request.Response),
            ct);

        return NoContent();
    }

    /// <summary>UC-003 — legal perfection status. Legal Team's own dashboard; Collateral Team gets read-only.</summary>
    [HttpPost("{id:guid}/perfection")]
    [RequireDashboard(
    DashboardModule.Legal,
    DashboardAccessLevel.Write)]
    public async Task<IActionResult> SubmitPerfection( Guid id,
    [FromBody] SubmitPerfectionRequest request,CancellationToken ct)
    {
        await _mediator.Send(
            new SubmitPerfectionCommand(
                id,
                request.CollateralValid,
                request.InvalidReason,
                request.RegistrationReference,
                request.LegalUnit),
            ct);

        return NoContent();
    }

    /// <summary>UC-004 — insurance policy capture. Insurance Team's own dashboard; Collateral Team gets read-only.</summary>
    [HttpPost("{id:guid}/insurance")]
    [RequireDashboard(
    DashboardModule.Insurance,
    DashboardAccessLevel.Write)]
    public async Task<IActionResult> SubmitInsurance(Guid id,[FromBody] SubmitInsuranceRequest request,CancellationToken ct)
    {
        await _mediator.Send(
            new SubmitInsuranceCommand(
                id,
                request.PolicyNumber,
                request.Insurer,
                request.CoverageAmount,
                request.PremiumAmount,
                request.PolicyStartDate,
                request.RenewalCadence),
            ct);

        return NoContent();
    }

    /// <summary>UC-006 — release + archive once the loan is fully repaid (checked against Vision, mocked).</summary>
    [HttpPost("{id:guid}/release")]
    [RequireDashboard(DashboardModule.Release, DashboardAccessLevel.Write)]
    public async Task<IActionResult> Release(Guid id, ReleaseCollateralRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ReleaseCollateralCommand(id, request.ReleasedBy, request.Notes), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/release/repayment-status")]
    [RequireDashboard(
    DashboardModule.Release,
    DashboardAccessLevel.Read)]
    public async Task<ActionResult<ReleaseRepaymentStatusVm>>GetReleaseRepaymentStatus(Guid id,CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetReleaseRepaymentStatusQuery(id),
            ct);

        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<CollateralSummaryVm>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCollateralSummaryQuery(), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/report")]
    public async Task<ActionResult<CollateralAssessmentReportVm>>GetAssessmentReport(Guid id,CancellationToken ct)
    {
        var report = await _mediator.Send(
            new GetCollateralAssessmentReportQuery(id),
            ct);

        return Ok(report);
    }

    [HttpGet("{id:guid}/report/pdf")]
    public async Task<IActionResult>DownloadAssessmentReportPdf(Guid id,CancellationToken ct)
    {
        var report =
            await _mediator.Send(
                new GetCollateralAssessmentReportQuery(id),
                ct);

        var file =
            _reportDocumentService.GeneratePdf(
                report);

        var fileName =
            $"Collateral-Assessment-{report.Header.LoanContractId}.pdf";

        return File(
            file,
            "application/pdf",
            fileName);
    }

    [HttpGet("{id:guid}/report/word")]
    public async Task<IActionResult>DownloadAssessmentReportWord(Guid id,CancellationToken ct)
    {
        var report =
            await _mediator.Send(
                new GetCollateralAssessmentReportQuery(id),
                ct);

        var file =
            _reportDocumentService.GenerateWord(
                report);

        var fileName =
            $"Collateral-Assessment-{report.Header.LoanContractId}.docx";

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            fileName);
    }

}

public sealed record SubmitPerfectionRequest(
    bool CollateralValid,
    string? InvalidReason,
    string RegistrationReference,
    string LegalUnit);
public sealed record SubmitInsuranceRequest(
    string PolicyNumber,
    string Insurer,
    decimal CoverageAmount,
    decimal PremiumAmount,
    DateTime PolicyStartDate,
    InsuranceRenewalCadence RenewalCadence);
public record ReleaseCollateralRequest(string ReleasedBy, string? Notes);
public sealed record SubmitValuationRequest(
    bool ManualEntry,

    decimal? ManualOpenMarketValue,
    decimal? ManualForcedSaleValue,
    string? ManualValuersName,
    DateTime? ManualValuationDate,

    string? ReportBase64,
    string? ReportContentType,

    ValuationDecision Decision,
    string Remark
);

public sealed record VerifyCollateralRequest(CollateralVerificationDecision Decision,string? Remark);
public sealed record ResubmitCollateralRequest(string Response);
public sealed record ResubmitValuationRequest(string Response);
