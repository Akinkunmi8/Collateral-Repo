
using CollateralPortal.Application.Collaterals.Queries.GetCollateralAssessmentReport;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Application.Documents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollateralPortal.Api.Controllers;

[ApiController]
[Route("api/collaterals/{collateralId:guid}/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly ICollateralDocumentService _documentService;


    public DocumentsController(ICollateralDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid collateralId,CancellationToken ct)
    {
        var documents = await _documentService.GetAllAsync(collateralId, ct);
        return Ok(documents);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Upload(Guid collateralId,[FromForm] UploadCollateralDocumentRequestDto request,CancellationToken ct)
    {
        await using var ms = new MemoryStream();

        await request.File.CopyToAsync(ms, ct);

        var command = new UploadCollateralDocumentCommand(
            collateralId,
            request.DocumentType,
            request.File.FileName,
            request.File.ContentType,
            ms.ToArray(),
            User.FindFirstValue(ClaimTypes.Email)
                ?? User.Identity?.Name
                ?? "unknown");

        var result = await _documentService.UploadAsync(command, ct);

        return Ok(result);
    }

    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> Download(Guid collateralId,Guid documentId,CancellationToken ct)
    {
        var document = await _documentService.DownloadAsync(collateralId, documentId, ct);

        if (document is null)
        {
            return NotFound();
        }

        return File(document.Content,document.ContentType,document.FileName);
    }

}