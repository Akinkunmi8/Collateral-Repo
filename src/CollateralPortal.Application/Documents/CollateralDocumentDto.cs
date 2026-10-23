using CollateralPortal.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollateralPortal.Application.Documents
{
    public sealed record CollateralDocumentDto(
    Guid Id,
    string FileName,
    CollateralDocumentType DocumentType,
    string ContentType,
    string UploadedBy,
    DateTime CreatedAtUtc);
}
