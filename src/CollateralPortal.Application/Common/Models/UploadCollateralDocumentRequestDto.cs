using CollateralPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollateralPortal.Application.Common.Models
{
    public class UploadCollateralDocumentRequestDto
    {
        public IFormFile File { get; set; } = default!;

        public CollateralDocumentType DocumentType { get; set; }
    }
}
