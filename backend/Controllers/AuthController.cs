using Microsoft.AspNetCore.Mvc;
using EmployeePortal.Models;
using EmployeePortal.Services;

namespace EmployeePortal.Controllers;

/// <summary>
/// Authentication flows: signup, login, logout, change-password.
/// Base route: /api/auth
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IBcService       _bc;
    private readonly IPasswordService _pwd;

    public AuthController(IBcService bc, IPasswordService pwd)
    {
        _bc  = bc;
        _pwd = pwd;
    }

    // ── POST /api/auth/signup ────────────────────────────────────────────────
    //
    // Flow:
    //   1. Verify employee exists in BC
    //   2. Reject if already registered (Portal_Password_Hash is not blank)
    //   3. Hash the password
    //   4. PATCH Portal_Password_Hash + Portal_Active = true in one call
    //   5. Return the same AuthData shape as login so the frontend can
    //      automatically log the user in right after signup
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.EmployeeNo) ||
            string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(ApiResponse.Fail("Employee Number and Password are required."));

        if (req.Password.Length < 8)
            return BadRequest(ApiResponse.Fail("Password must be at least 8 characters."));

        try
        {
            var employee = await _bc.GetEmployeeByNoAsync(req.EmployeeNo);

            // Don't reveal whether the employee number exists — use a generic message
            if (employee is null)
                return NotFound(ApiResponse.Fail(
                    "No employee record found for that Employee Number. Please contact HR."));

            var root         = employee.Value;
            string storedHash = GetString(root, "Portal_Password_Hash");

            // Already registered — hash is not blank
            if (!string.IsNullOrWhiteSpace(storedHash))
                return Conflict(ApiResponse.Fail(
                    "This Employee Number is already registered. Please log in instead."));

            // Hash the password and write it to BC, flipping Portal_Active on at the same time
            string newHash = _pwd.Hash(req.Password);
            string etag    = await _bc.GetEtagAsync(req.EmployeeNo);
            await _bc.RegisterEmployeeAsync(req.EmployeeNo, newHash, etag);

            // Build the auth response from the record we already have
            // (no extra GET needed — we only just read it above)
            var data = BuildAuthData(req.EmployeeNo, root);

            return StatusCode(201, ApiResponse<AuthData>.Ok(
                "Account created successfully. You are now logged in.", data));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail($"Internal server error: {ex.Message}"));
        }
    }

    // ── POST /api/auth/login ─────────────────────────────────────────────────
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.EmployeeNo) ||
            string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(ApiResponse.Fail("Employee Number and Password are required."));

        try
        {
            var employee = await _bc.GetEmployeeByNoAsync(req.EmployeeNo);

            if (employee is null)
                return Unauthorized(ApiResponse.Fail("Invalid Employee Number or Password."));

            var root = employee.Value;

            string storedHash    = GetString(root, "Portal_Password_Hash");
            bool   isPortalActive = root.TryGetProperty("Portal_Active", out var ap) && ap.GetBoolean();

            // Account exists in BC but has never been registered through the portal
            if (string.IsNullOrWhiteSpace(storedHash))
                return Unauthorized(ApiResponse.Fail(
                    "No portal account found. Please sign up first."));

            // HR-deactivated account (e.g. suspended or offboarded)
            if (!isPortalActive)
                return StatusCode(403, ApiResponse.Fail(
                    "Your portal account is not active. Please contact HR."));

            if (!_pwd.Verify(req.Password, storedHash))
                return Unauthorized(ApiResponse.Fail("Invalid Employee Number or Password."));

            return Ok(ApiResponse<AuthData>.Ok("Login successful.", BuildAuthData(req.EmployeeNo, root)));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail($"Internal server error: {ex.Message}"));
        }
    }

    // ── POST /api/auth/logout ────────────────────────────────────────────────
    // Stateless — the server has nothing to invalidate right now.
    // The frontend clears its token/session on receiving this 200.
    // When you add JWT later, add the token to a blacklist here.
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    // ── PUT /api/auth/change-password ────────────────────────────────────────
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.EmployeeNo)       ||
            string.IsNullOrWhiteSpace(req.CurrentPassword)  ||
            string.IsNullOrWhiteSpace(req.NewPassword))
            return BadRequest(ApiResponse.Fail(
                "Employee number, current password, and new password are all required."));

        if (req.NewPassword.Length < 8)
            return BadRequest(ApiResponse.Fail("New password must be at least 8 characters."));

        if (req.CurrentPassword == req.NewPassword)
            return BadRequest(ApiResponse.Fail(
                "New password must be different from your current password."));

        try
        {
            var employee = await _bc.GetEmployeeByNoAsync(req.EmployeeNo);

            if (employee is null)
                return NotFound(ApiResponse.Fail("Employee not found."));

            var root = employee.Value;

            bool isPortalActive = root.TryGetProperty("Portal_Active", out var ap) && ap.GetBoolean();
            if (!isPortalActive)
                return StatusCode(403, ApiResponse.Fail("Your portal account is not active."));

            string storedHash = GetString(root, "Portal_Password_Hash");

            // Guard: can't change password if never registered
            if (string.IsNullOrWhiteSpace(storedHash))
                return BadRequest(ApiResponse.Fail(
                    "No portal account found. Please sign up first."));

            if (!_pwd.Verify(req.CurrentPassword, storedHash))
                return Unauthorized(ApiResponse.Fail("Current password is incorrect."));

            string newHash = _pwd.Hash(req.NewPassword);
            string etag    = await _bc.GetEtagAsync(req.EmployeeNo);
            await _bc.UpdatePasswordHashAsync(req.EmployeeNo, newHash, etag);

            return Ok(ApiResponse.Ok("Password changed successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail($"Internal server error: {ex.Message}"));
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static AuthData BuildAuthData(string employeeNo, System.Text.Json.JsonElement root) =>
        new()
        {
            EmployeeNo = employeeNo,
            FirstName  = GetString(root, "First_Name"),
            LastName   = GetString(root, "Last_Name"),
            Email      = GetString(root, "Company_E_Mail"),
            JobTitle   = GetString(root, "Job_Title"),
            PhoneNo    = GetString(root, "Phone_No"),
        };

    private static string GetString(System.Text.Json.JsonElement root, string key) =>
        root.TryGetProperty(key, out var prop) ? prop.GetString() ?? "" : "";
}