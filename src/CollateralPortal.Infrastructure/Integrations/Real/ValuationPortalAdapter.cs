using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CollateralPortal.Infrastructure.Integrations.Real;

/// <summary>
/// Real adapter for the Valuation Portal:
/// GET {BaseUrl}/api/ValuationReport/completed-by-account/{accountNumber}
/// Returns 200 + CompletedValuationReportDto-shaped body when a completed
/// valuation exists for the account, otherwise treat as "not found" (null) —
/// the calling command decides whether that means "not ready yet" or
/// "needs manual entry".
/// </summary>
public class ValuationPortalAdapter : IValuationPortalClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<ValuationPortalAdapter> _logger;

    public ValuationPortalAdapter(HttpClient httpClient, ILogger<ValuationPortalAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ValuationReportDto?> GetValuationReportAsync(string accountNumber, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/ValuationReport/completed-by-account/{Uri.EscapeDataString(accountNumber)}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("No completed valuation report found for account {AccountNumber}", accountNumber);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Valuation Portal returned {StatusCode} for account {AccountNumber}", response.StatusCode, accountNumber);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ValuationReportDto>(JsonOptions, ct);
    }
}
