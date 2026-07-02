using Microsoft.AspNetCore.Mvc;
using EmployeePortal.Services;

namespace EmployeePortal.Controllers;

/// <summary>
/// Handles employee read operations.
/// Base route: /api/employees
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IBcService _bcService;

    public EmployeesController(IBcService bcService)
    {
        _bcService = bcService;
    }

    // GET /api/employees
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var json = await _bcService.GetAllEmployeesAsync();

            if (json is null)
                return StatusCode(502, new { message = "Could not retrieve employees from Business Central." });

            // Return the raw BC JSON directly so the shape is unchanged for the frontend
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    // GET /api/employees/{employeeNo}
    [HttpGet("{employeeNo}")]
    public async Task<IActionResult> GetOne(string employeeNo)
    {
        try
        {
            var employee = await _bcService.GetEmployeeByNoAsync(employeeNo);

            if (employee is null)
                return NotFound(new { message = $"Employee '{employeeNo}' not found." });

            return Ok(employee);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }
}