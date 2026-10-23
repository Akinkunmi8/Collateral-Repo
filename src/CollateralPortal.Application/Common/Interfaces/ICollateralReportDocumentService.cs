using CollateralPortal.Application.Collaterals
    .Queries.GetCollateralAssessmentReport;

namespace CollateralPortal.Application.Common.Interfaces;

public interface ICollateralReportDocumentService
{
    byte[] GeneratePdf(CollateralAssessmentReportVm report);

    byte[] GenerateWord(CollateralAssessmentReportVm report);
}