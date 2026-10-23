using CollateralPortal.Application.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollateralPortal.Application.Common.Interfaces
{
    public interface ICollateralDocumentService
    {
        Task<IReadOnlyList<CollateralDocumentDto>> GetAllAsync(
            Guid collateralId,
            CancellationToken cancellationToken = default);

        Task<UploadCollateralDocumentResult> UploadAsync(
            UploadCollateralDocumentCommand command,
            CancellationToken cancellationToken = default);

        Task<CollateralDocumentDownloadDto?> DownloadAsync(
            Guid collateralId,
            Guid documentId,
            CancellationToken cancellationToken = default);
    }
}
