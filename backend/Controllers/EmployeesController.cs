using Microsoft.AspNetCore.Mvc;
using EmployeePortal.Models;
using EmployeePortal.Services;

namespace EmployeePortal.Controllers;

/// <summary>
/// Employee read and profile-update operations.
/// Base route: /api/employees
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IBcService _bc;

    public EmployeesController(IBcService bc) => _bc = bc;

    // ── GET /api/employees ───────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var json = await _bc.GetAllEmployeesAsync();

            if (json is null)
                return StatusCode(502, ApiResponse.Fail(
                    "Could not retrieve employees from Business Central."));

            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail($"Internal server error: {ex.Message}"));
        }
    }

    // ── GET /api/employees/{employeeNo} ──────────────────────────────────────
    [HttpGet("{employeeNo}")]
    public async Task<IActionResult> GetOne(string employeeNo)
    {
        try
        {
            var employee = await _bc.GetEmployeeByNoAsync(employeeNo);

            if (employee is null)
                return NotFound(ApiResponse.Fail($"Employee '{employeeNo}' not found."));

            return Ok(ApiResponse<EmployeeData>.Ok(
                "Employee retrieved.", MapToEmployeeData(employeeNo, employee.Value)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail($"Internal server error: {ex.Message}"));
        }
    }

    // ── PUT /api/employees/{employeeNo} ──────────────────────────────────────
    // Only non-null fields in the request body are patched — omitted fields
    // are left unchanged in BC. The response returns the full updated record.
    [HttpPut("{employeeNo}")]
    public async Task<IActionResult> UpdateEmployee(
        string employeeNo,
        [FromBody] UpdateEmployeeRequest req)
    {
        bool hasAnyField =
            req.FirstName    is not null ||
            req.LastName     is not null ||
            req.CompanyEmail is not null ||
            req.PhoneNo      is not null ||
            req.JobTitle     is not null;

        if (!hasAnyField)
            return BadRequest(ApiResponse.Fail("No fields provided to update."));

        if (req.CompanyEmail is not null && !req.CompanyEmail.Contains('@'))
            return BadRequest(ApiResponse.Fail("Invalid email address format."));

        try
        {
            var existing = await _bc.GetEmployeeByNoAsync(employeeNo);
            if (existing is null)
                return NotFound(ApiResponse.Fail($"Employee '{employeeNo}' not found."));

            string etag = await _bc.GetEtagAsync(employeeNo);
            await _bc.UpdateEmployeeAsync(employeeNo, req, etag);

            // Re-fetch to return the live updated record
            var updated = await _bc.GetEmployeeByNoAsync(employeeNo);

            return Ok(ApiResponse<EmployeeData>.Ok(
                "Employee details updated successfully.",
                MapToEmployeeData(employeeNo, updated!.Value)));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail($"Internal server error: {ex.Message}"));
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static EmployeeData MapToEmployeeData(string employeeNo, System.Text.Json.JsonElement root) =>
        new()
        {
            EmployeeNo = employeeNo,
            FirstName  = GetString(root, "First_Name"),
            LastName   = GetString(root, "Last_Name"),
            Email      = GetString(root, "Company_E_Mail"),
            JobTitle   = GetString(root, "Job_Title"),
            PhoneNo    = GetString(root, "Phone_No"),
            IsActive   = root.TryGetProperty("Portal_Active", out var ap) && ap.GetBoolean(),
        };

    private static string GetString(System.Text.Json.JsonElement root, string key) =>
        root.TryGetProperty(key, out var prop) ? prop.GetString() ?? "" : "";
}