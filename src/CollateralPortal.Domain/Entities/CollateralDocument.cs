using CollateralPortal.Domain.Common;
using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Domain.Entities;

public class CollateralDocument : BaseAuditableEntity
{
    public Guid CollateralId { get; set; }

    public Collateral? Collateral { get; set; }

    public CollateralDocumentType DocumentType { get; set; }

    /// <summary>
    /// Original filename supplied by the user.
    /// Example: GovernorConsent.pdf
    /// </summary>
    public string FileName { get; set; } = default!;

    /// <summary>
    /// MIME type of the uploaded document.
    /// Example: application/pdf
    /// </summary>
    public string ContentType { get; set; } =
        "application/octet-stream";

    /// <summary>
    /// Relative location of the physical file.
    ///
    /// Example:
    /// Collaterals/{collateralId}/LegalPerfection/{guid}.pdf
    ///
    /// Never expose the physical server path to the client.
    /// </summary>
    public string? RelativePath { get; set; }

    /// <summary>
    /// Size of the physical file in bytes.
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Legacy database file content.
    ///
    /// Kept temporarily so existing documents are not broken.
    /// New uploads should use RelativePath instead.
    /// </summary>
    public byte[]? Content { get; set; }

    public string UploadedBy { get; set; } = default!;
}