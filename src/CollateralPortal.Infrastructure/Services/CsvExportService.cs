using System.Globalization;
using System.Text;
using CollateralPortal.Application.Common.Interfaces;

namespace CollateralPortal.Infrastructure.Services;

public sealed class CsvExportService : ICsvExportService
{
    public byte[] Generate<T>(IReadOnlyCollection<T> records, IReadOnlyCollection<ICsvExportService.CsvColumn<T>> columns)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            string.Join(
                ",",
                columns.Select(x => Escape(x.Header)))
        );

        foreach (var record in records)
        {
            var values = columns
                .Select(column =>
                    FormatValue(column.Value(record)))
                .Select(Escape);

            builder.AppendLine(
                string.Join(",", values)
            );
        }

        // UTF-8 BOM improves Excel compatibility.
        var preamble = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(
            builder.ToString());

        var result =
            new byte[preamble.Length + content.Length];

        Buffer.BlockCopy(
            preamble,
            0,
            result,
            0,
            preamble.Length);

        Buffer.BlockCopy(
            content,
            0,
            result,
            preamble.Length,
            content.Length);

        return result;
    }

    private static string FormatValue(object? value)
    {
        if (value is null)
            return string.Empty;

        return value switch
        {
            DateTime date =>
                date.ToString(
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture),

            DateTimeOffset date =>
                date.ToString(
                    "yyyy-MM-dd HH:mm:ss zzz",
                    CultureInfo.InvariantCulture),

            decimal number =>
                number.ToString(
                    CultureInfo.InvariantCulture),

            double number =>
                number.ToString(
                    CultureInfo.InvariantCulture),

            float number =>
                number.ToString(
                    CultureInfo.InvariantCulture),

            bool boolean =>
                boolean ? "Yes" : "No",

            _ => Convert.ToString(
                     value,
                     CultureInfo.InvariantCulture)
                 ?? string.Empty
        };
    }

    private static string Escape(string value)
    {
        // Prevent Excel/CSV formula injection.
        if (!string.IsNullOrWhiteSpace(value))
        {
            var trimmed = value.TrimStart();

            if (trimmed.StartsWith("=") ||
                trimmed.StartsWith("+") ||
                trimmed.StartsWith("-") ||
                trimmed.StartsWith("@"))
            {
                value = "'" + value;
            }
        }

        value = value.Replace("\"", "\"\"");

        return $"\"{value}\"";
    }
}