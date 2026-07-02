using System.Text.Json;
using EmployeePortal.Models;

namespace EmployeePortal.Services;

public interface IBcService
{
    /// <summary>Returns all employees as a raw JSON string.</summary>
    Task<string?> GetAllEmployeesAsync();

    /// <summary>Returns a single employee's JSON element, or null if not found.</summary>
    Task<JsonElement?> GetEmployeeByNoAsync(string employeeNo);

    /// <summary>
    /// Registers an employee for portal access: sets Portal_Password_Hash
    /// and flips Portal_Active to true in a single PATCH.
    /// </summary>
    Task RegisterEmployeeAsync(string employeeNo, string passwordHash, string etag);

    /// <summary>PATCHes Portal_Password_Hash only (used by change-password).</summary>
    Task UpdatePasswordHashAsync(string employeeNo, string newHash, string etag);

    /// <summary>PATCHes editable profile fields. Only non-null fields are sent.</summary>
    Task UpdateEmployeeAsync(string employeeNo, UpdateEmployeeRequest request, string etag);

    /// <summary>
    /// Fetches the current OData ETag for a record.
    /// BC requires this header on every PATCH to handle concurrency.
    /// </summary>
    Task<string> GetEtagAsync(string employeeNo);
}