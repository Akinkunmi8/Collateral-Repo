using CollateralPortal.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollateralPortal.Application.Documents
{
    public sealed record UploadCollateralDocumentCommand(
    Guid CollateralId,
    CollateralDocumentType DocumentType,
    string FileName,
    string ContentType,
    byte[] Content,
    string UploadedBy);
    public sealed record UploadCollateralDocumentResult(Guid DocumentId);
}
