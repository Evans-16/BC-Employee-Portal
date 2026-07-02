using System.Net;
using System.Text.Json;

namespace EmployeePortal.Services;

/// <summary>
/// Handles all raw HTTP communication with Business Central OData endpoints.
/// Controllers call this service instead of building HttpClients themselves.
/// </summary>
public class BcService : IBcService
{
    private readonly IHttpClientFactory _httpClientFactory;

    // Base OData URL — move this to appsettings.json when you're ready
    private const string BaseUrl =
        "http://w1n:7048/BC270/ODataV4/Company('CRONUS%20International%20Ltd.')/Employees";

    public BcService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc/>
    public async Task<string?> GetAllEmployeesAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync(BaseUrl);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();

        return null;
    }

    /// <inheritdoc/>
    public async Task<JsonElement?> GetEmployeeByNoAsync(string employeeNo)
    {
        using var client = CreateClient();
        var url = $"{BaseUrl}('{employeeNo}')";
        var response = await client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"BC returned {(int)response.StatusCode}: {response.ReasonPhrase}");

        var json = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(json);

        // Return a *clone* so the JsonDocument can be safely disposed
        return doc.RootElement.Clone();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private HttpClient CreateClient()
    {
        // DefaultNetworkCredentials = Windows auth (NTLM/Kerberos) — fine for
        // an on-prem BC instance on a domain-joined server.
        var handler = new HttpClientHandler
        {
            Credentials = CredentialCache.DefaultNetworkCredentials
        };
        return new HttpClient(handler);
    }
}