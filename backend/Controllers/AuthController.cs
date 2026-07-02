using Microsoft.AspNetCore.Mvc;
using EmployeePortal.Models;
using EmployeePortal.Services;

namespace EmployeePortal.Controllers;

/// <summary>
/// Handles all authentication flows (login, logout, password change, etc.)
/// Base route: /api/auth
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IBcService _bcService;

    public AuthController(IBcService bcService)
    {
        _bcService = bcService;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Basic validation (ModelState handles nulls from [ApiController])
        if (string.IsNullOrWhiteSpace(request.EmployeeNo) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Employee Number and Password are required." });
        }

        try
        {
            var employee = await _bcService.GetEmployeeByNoAsync(request.EmployeeNo);

            // No record found → treat as wrong credentials (don't reveal which field is wrong)
            if (employee is null)
                return Unauthorized(new { message = "Invalid Employee Number or Password." });

            var root = employee.Value;

            // Extract fields safely
            string bcPasswordHash = root.TryGetProperty("Portal_Password_Hash", out var passProp)
                ? passProp.GetString() ?? "" : "";

            bool isPortalActive = root.TryGetProperty("Portal_Active", out var activeProp)
                && activeProp.GetBoolean();

            string firstName = root.TryGetProperty("First_Name", out var nameProp)
                ? nameProp.GetString() ?? "" : "";

            string email = root.TryGetProperty("Company_Email", out var emailProp)
                ? emailProp.GetString() ?? "" : "";

            // 1. Portal must be activated by HR before login is allowed
            if (!isPortalActive)
                return StatusCode(403, new { message = "Your portal account is not active. Please contact HR." });

            // 2. Password check
            // TODO: replace with BCrypt.Net or PBKDF2 hash comparison before going to production
            if (bcPasswordHash != request.Password)
                return Unauthorized(new { message = "Invalid Employee Number or Password." });

            // Success
            return Ok(new LoginResponse
            {
                Message    = "Login successful!",
                EmployeeNo = request.EmployeeNo,
                FirstName  = firstName,
                Email      = email
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }
}