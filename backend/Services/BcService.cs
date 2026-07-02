using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EmployeePortal.Models;

namespace EmployeePortal.Services;

/// <summary>
/// All raw HTTP communication with Business Central OData v4.
/// One instance per request (Scoped lifetime).
/// </summary>
public class BcService : IBcService
{
    private readonly IHttpClientFactory _factory;

    // TODO: move to appsettings.json / environment variables before production
    private const string BaseUrl =
        "http://w1n:7048/BC270/ODataV4/Company('CRONUS%20International%20Ltd.')/Employees";

    public BcService(IHttpClientFactory factory) => _factory = factory;

    // ── Reads ─────────────────────────────────────────────────────────────────

    public async Task<string?> GetAllEmployeesAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync(BaseUrl);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsStringAsync()
            : null;
    }

    public async Task<JsonElement?> GetEmployeeByNoAsync(string employeeNo)
    {
        using var client = CreateClient();
        var response = await client.GetAsync(EmployeeUrl(employeeNo));

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        EnsureSuccess(response, "fetch employee");

        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    public async Task<string> GetEtagAsync(string employeeNo)
    {
        using var client = CreateClient();
        var response = await client.GetAsync(EmployeeUrl(employeeNo));

        EnsureSuccess(response, "fetch ETag");

        // Prefer the response header; fall back to the @odata.etag property in the body
        if (response.Headers.ETag?.Tag is { } tag) return tag;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("@odata.etag", out var etagProp))
        {
            var raw = etagProp.GetString();
            if (!string.IsNullOrEmpty(raw))
                return NormalizeEtag(raw);
        }

        return "*"; // wildcard — BC accepts it but skips concurrency checking
    }

    // ── Writes ────────────────────────────────────────────────────────────────

    public async Task RegisterEmployeeAsync(string employeeNo, string passwordHash, string etag)
    {
        // Single PATCH sets both the hash AND activates the account atomically
        var payload = new
        {
            Portal_Password_Hash = passwordHash,
            Portal_Active        = true
        };
        await PatchAsync(employeeNo, payload, etag, "register employee");
    }

    public async Task UpdatePasswordHashAsync(string employeeNo, string newHash, string etag)
    {
        var payload = new { Portal_Password_Hash = newHash };
        await PatchAsync(employeeNo, payload, etag, "update password");
    }

    public async Task UpdateEmployeeAsync(string employeeNo, UpdateEmployeeRequest req, string etag)
    {
        var fields = new Dictionary<string, object?>();

        if (req.FirstName    is not null) fields["First_Name"]    = req.FirstName;
        if (req.LastName     is not null) fields["Last_Name"]     = req.LastName;
        if (req.CompanyEmail is not null) fields["Company_E_Mail"] = req.CompanyEmail;
        if (req.PhoneNo      is not null) fields["Phone_No"]      = req.PhoneNo;
        if (req.JobTitle     is not null) fields["Job_Title"]     = req.JobTitle;

        if (fields.Count == 0)
            throw new ArgumentException("No fields provided to update.");

        await PatchAsync(employeeNo, fields, etag, "update employee");
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task PatchAsync(string employeeNo, object payload, string etag, string operation)
    {
        using var client = CreateClient();

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        client.DefaultRequestHeaders.IfMatch.Add(BuildIfMatchHeader(etag));

        var response = await client.PatchAsync(EmployeeUrl(employeeNo), content);
        EnsureSuccess(response, operation);
    }

    // Builds a valid If-Match header value. Falls back to "match any" if the
    // etag we were given can't be parsed as a valid RFC 7232 quoted string,
    // instead of letting EntityTagHeaderValue throw and bubble up as a 500.
    private static EntityTagHeaderValue BuildIfMatchHeader(string etag)
    {
        if (etag == "*")
            return EntityTagHeaderValue.Any;

        try
        {
            return new EntityTagHeaderValue(NormalizeEtag(etag));
        }
        catch (FormatException)
        {
            return EntityTagHeaderValue.Any;
        }
    }

    // Ensures the etag is wrapped in literal double quotes as RFC 7232 requires,
    // so EntityTagHeaderValue doesn't throw "The specified value is not a valid
    // quoted string." This is needed because BC's @odata.etag JSON property
    // sometimes comes back without the surrounding quote characters.
    private static string NormalizeEtag(string raw)
    {
        if (raw.StartsWith("W/\"") && raw.EndsWith("\"")) return raw; // weak etag, already fine
        if (raw.StartsWith("\"") && raw.EndsWith("\""))    return raw; // already fine
        return $"\"{raw}\"";
    }

    private static HttpClient CreateClient() =>
        new(new HttpClientHandler
        {
            Credentials = CredentialCache.DefaultNetworkCredentials
        });

    private static string EmployeeUrl(string employeeNo)
    {
        var escaped = employeeNo.Trim().Replace("'", "''"); // OData literal escaping
        return $"{BaseUrl}('{Uri.EscapeDataString(escaped)}')";
    }

    private static void EnsureSuccess(HttpResponseMessage r, string op)
    {
        if (!r.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"BC returned {(int)r.StatusCode} during '{op}': {r.ReasonPhrase}");
    }
}