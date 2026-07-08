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

    /// <summary>
    /// Returns the RAW etag string, unmodified — either from the response
    /// header (already correctly parsed by .NET, no "W/" prefix, already
    /// quoted) or from the JSON body's @odata.etag property (which DOES
    /// still contain a literal "W/" prefix as plain text, since it's just a
    /// JSON string value). All the "is it quoted, does it have W/" handling
    /// happens later in BuildIfMatchHeader — this method just fetches it.
    /// </summary>
    public async Task<string> GetEtagAsync(string employeeNo)
    {
        using var client = CreateClient();
        var response = await client.GetAsync(EmployeeUrl(employeeNo));

        EnsureSuccess(response, "fetch ETag");

        if (response.Headers.ETag?.Tag is { } tag) return tag;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("@odata.etag", out var etagProp))
            return etagProp.GetString() ?? "*";

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

        if (req.FirstName      is not null) fields["First_Name"]      = req.FirstName;
        if (req.LastName       is not null) fields["Last_Name"]       = req.LastName;
        if (req.CompanyEmail   is not null) fields["Company_E_Mail"]  = req.CompanyEmail;
        if (req.PhoneNo        is not null) fields["Phone_No"]        = req.PhoneNo;
        if (req.JobTitle       is not null) fields["Job_Title"]       = req.JobTitle;
        if (req.Gender         is not null) fields["Gender"]          = req.Gender;
        if (req.EmploymentType is not null) fields["Engagement_Type"] = req.EmploymentType;

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

    /// <summary>
    /// Builds a correct If-Match header from a raw etag string, regardless of
    /// which shape it arrived in:
    ///   - "*"                              → match-any
    ///   - "abc123"                         → unquoted tag, needs quotes added
    ///   - "\"abc123\""                     → already a valid quoted tag
    ///   - "W/\"abc123\""                   → weak tag with the "W/" baked in
    ///                                        as literal text (this is the
    ///                                        shape that broke before: the
    ///                                        old code quoted the WHOLE
    ///                                        string, producing something
    ///                                        like "\"W/\"abc123\"\"" with
    ///                                        unescaped inner quotes, which
    ///                                        .NET rejects).
    /// The "W/" prefix and the quoted tag are handled as two separate things
    /// — EntityTagHeaderValue takes the tag and the weak-flag as separate
    /// constructor arguments, it does NOT want "W/" inside the tag string.
    /// </summary>
    private static EntityTagHeaderValue BuildIfMatchHeader(string etag)
    {
        if (string.IsNullOrWhiteSpace(etag) || etag == "*")
            return EntityTagHeaderValue.Any;

        var value  = etag.Trim();
        bool isWeak = value.StartsWith("W/", StringComparison.OrdinalIgnoreCase);
        if (isWeak) value = value.Substring(2).Trim();

        if (!value.StartsWith('"')) value = "\"" + value;
        if (!value.EndsWith('"'))   value = value + "\"";

        return new EntityTagHeaderValue(value, isWeak);
    }

    private static void EnsureSuccess(HttpResponseMessage r, string op)
    {
        if (!r.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"BC returned {(int)r.StatusCode} during '{op}': {r.ReasonPhrase}");
    }
}