using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Infrastructure.Integrations.Mocks;

public sealed class MockValuationPortalClient : IValuationPortalClient
{
    /*
     * Keep mock metadata static.
     *
     * Do NOT load files/Base64 during static initialization.
     */
    private static readonly IReadOnlyList<ValuationReportDto> Reports =
    [
        new ValuationReportDto(
            FacilityRef: "LN-2026-00123",
            FacilityAmount: 50_000_000m,
            Status: "Completed",
            OperativeAcc: "1000000000",
            State: "Lagos",
            LocalGov: "Ikeja",
            PropertyDescription:
                "Commercial property pledged as security for term loan facility",
            Landsize: "1,250 sqm",
            ValuationDate: new DateTime(2026, 9, 19),
            OpenMarketValue: 85_000_000m,
            ForceSalesValue: 70_000_000m,
            InsuranceValue: 80_000_000m,
            SalesLocation: "Ikeja, Lagos",
            SalesDate: null,
            ValuationReportName: "VAL-LN-2026-00123.pdf",
            ValuersName: "Akin Realtors",
            DateAssigned: new DateTime(2026, 9, 17),

            // Important:
            // Never perform file I/O in this static collection.
            ValuationReportBase64: null,

            ContentType: "application/pdf"
        ),

        new ValuationReportDto(
            FacilityRef: "LN-2026-00456",
            FacilityAmount: 15_000_000m,
            Status: "Completed",
            OperativeAcc: "2000000111",
            State: "Kano",
            LocalGov: "Nassarawa",
            PropertyDescription:
                "Commercial property pledged as security for term loan facility",
            Landsize: "850 sqm",
            ValuationDate: new DateTime(2026, 9, 18),
            OpenMarketValue: 30_000_000m,
            ForceSalesValue: 22_500_000m,
            InsuranceValue: 27_000_000m,
            SalesLocation: "Kano",
            SalesDate: null,
            ValuationReportName: "VAL-LN-2026-00456.pdf",
            ValuersName: "Akin Realtors",
            DateAssigned: new DateTime(2026, 9, 16),
            ValuationReportBase64: null,
            ContentType: "application/pdf"
        ),

        new ValuationReportDto(
            FacilityRef: "LN-2026-00789",
            FacilityAmount: 120_000_000m,
            Status: "Completed",
            OperativeAcc: "3000000222",
            State: "Abuja",
            LocalGov: "Municipal Area Council",
            PropertyDescription:
                "Commercial logistics vehicles pledged as security for asset finance facility",
            Landsize: null,
            ValuationDate: new DateTime(2026, 9, 20),
            OpenMarketValue: 180_000_000m,
            ForceSalesValue: 160_000_000m,
            InsuranceValue: 170_000_000m,
            SalesLocation: "Idu Industrial Area, Abuja",
            SalesDate: null,
            ValuationReportName: "VAL-LN-2026-00789.pdf",
            ValuersName: "Prime Assets Valuation Ltd",
            DateAssigned: new DateTime(2026, 9, 18),
            ValuationReportBase64: null,
            ContentType: "application/pdf"
        )
    ];

    public Task<ValuationReportDto?> GetValuationReportAsync(
        string accountNumber,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return Task.FromResult<ValuationReportDto?>(null);
        }

        var source = Reports.FirstOrDefault(x =>
            string.Equals(
                x.OperativeAcc,
                accountNumber.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (source is null)
        {
            Console.WriteLine(
                $"[MOCK VALUATION] No report found for account: {accountNumber}");

            return Task.FromResult<ValuationReportDto?>(null);
        }

        /*
         * Read the report at request time.
         *
         * This avoids TypeInitializationException and ensures
         * that an updated mock PDF is picked up without storing
         * its bytes inside the static Reports collection.
         */
        var reportBase64 = GetSampleReportBase64();

        Console.WriteLine(
            $"[MOCK VALUATION] Account: {accountNumber}");

        Console.WriteLine(
            $"[MOCK VALUATION] Report: {source.ValuationReportName}");

        Console.WriteLine(
            $"[MOCK VALUATION] Base64 length: {reportBase64.Length}");

        /*
         * ValuationReportDto is a record, so clone the metadata
         * and attach the report returned by the mock portal.
         */
        var result = source with
        {
            ValuationReportBase64 = reportBase64
        };

        return Task.FromResult<ValuationReportDto?>(result);
    }

    private static string GetSampleReportBase64()
    {
        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            "MockData",
            "SampleValuationReport.pdf");

        Console.WriteLine(
            $"[MOCK VALUATION PDF] Runtime path: {filePath}");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Mock valuation report was not found at: {filePath}",
                filePath);
        }

        var fileBytes =
            File.ReadAllBytes(filePath);

        Console.WriteLine(
            $"[MOCK VALUATION PDF] Bytes read: {fileBytes.Length}");

        if (fileBytes.Length == 0)
        {
            throw new InvalidOperationException(
                $"Mock valuation report exists but is empty: {filePath}");
        }

        var base64 =
            Convert.ToBase64String(fileBytes);

        Console.WriteLine(
            $"[MOCK VALUATION PDF] Base64 length: {base64.Length}");

        return base64;
    }
}