namespace CollateralPortal.Application.Common.Interfaces;

public interface ICsvExportService
{
    byte[] Generate<T>(IReadOnlyCollection<T> records,IReadOnlyCollection<CsvColumn<T>> columns);

    public sealed record CsvColumn<T>(string Header,Func<T, object?> Value);
}