using System.Text.Json;

namespace EmployeePortal.Services;

/// <summary>
/// Abstracts all HTTP calls to Business Central OData endpoints.
/// Controllers depend on this interface, not the concrete class,
/// making it easy to mock in tests later.
/// </summary>
public interface IBcService
{
    /// <summary>Returns all employees as a raw JSON string.</summary>
    Task<string?> GetAllEmployeesAsync();

    /// <summary>
    /// Returns a single employee's root JSON element, or null if not found.
    /// </summary>
    Task<JsonElement?> GetEmployeeByNoAsync(string employeeNo);
}