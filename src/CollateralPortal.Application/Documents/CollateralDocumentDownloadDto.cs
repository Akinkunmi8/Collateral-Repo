using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollateralPortal.Application.Documents
{
    public sealed record CollateralDocumentDownloadDto(
    byte[] Content,
    string ContentType,
    string FileName);
}
