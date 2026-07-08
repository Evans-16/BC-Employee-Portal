using Microsoft.AspNetCore.Mvc;
using EmployeePortal.Models;
using EmployeePortal.Services;

namespace EmployeePortal.Controllers;

/// <summary>
/// Authentication flows: signup, login, logout, change-password, forgot-password.
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

            if (employee is null)
                return NotFound(ApiResponse.Fail(
                    "No employee record found for that Employee Number. Please contact HR."));

            var root         = employee.Value;
            string storedHash = GetString(root, "Portal_Password_Hash");

            if (!string.IsNullOrWhiteSpace(storedHash))
                return Conflict(ApiResponse.Fail(
                    "This Employee Number is already registered. Please log in instead."));

            string newHash = _pwd.Hash(req.Password);
            string etag    = await _bc.GetEtagAsync(req.EmployeeNo);
            await _bc.RegisterEmployeeAsync(req.EmployeeNo, newHash, etag);

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

            string storedHash     = GetString(root, "Portal_Password_Hash");
            bool   isPortalActive = root.TryGetProperty("Portal_Active", out var ap) && ap.GetBoolean();

            if (string.IsNullOrWhiteSpace(storedHash))
                return Unauthorized(ApiResponse.Fail(
                    "No portal account found. Please sign up first."));

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
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    // ── PUT /api/auth/change-password ────────────────────────────────────────
    // Requires the CURRENT password — used from inside the dashboard.
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

    // ── PUT /api/auth/forgot-password ────────────────────────────────────────
    // Doesn't require the current password. Instead verifies identity via
    // Employee Number + National ID (BC's Social_Security_No field), since
    // there's no email/SMS service wired up yet to send a reset link/code.
    //
    // Deliberately returns the SAME generic error whether the employee
    // doesn't exist or the National ID doesn't match — this avoids leaking
    // which Employee Numbers are valid to someone probing the endpoint.
    [HttpPut("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.EmployeeNo) ||
            string.IsNullOrWhiteSpace(req.NationalId) ||
            string.IsNullOrWhiteSpace(req.NewPassword))
            return BadRequest(ApiResponse.Fail(
                "Employee number, National ID, and new password are all required."));

        if (req.NewPassword.Length < 8)
            return BadRequest(ApiResponse.Fail("New password must be at least 8 characters."));

        const string genericFailure = "Employee Number and National ID do not match our records.";

        try
        {
            var employee = await _bc.GetEmployeeByNoAsync(req.EmployeeNo);

            if (employee is null)
                return Unauthorized(ApiResponse.Fail(genericFailure));

            var root = employee.Value;

            string storedHash = GetString(root, "Portal_Password_Hash");
            if (string.IsNullOrWhiteSpace(storedHash))
                return BadRequest(ApiResponse.Fail(
                    "No portal account found for this Employee Number. Please sign up first."));

            bool isPortalActive = root.TryGetProperty("Portal_Active", out var ap) && ap.GetBoolean();
            if (!isPortalActive)
                return StatusCode(403, ApiResponse.Fail("Your portal account is not active. Please contact HR."));

            string storedNationalId = GetString(root, "Social_Security_No").Trim();
            string suppliedNationalId = req.NationalId.Trim();

            if (string.IsNullOrWhiteSpace(storedNationalId) ||
                !string.Equals(storedNationalId, suppliedNationalId, StringComparison.OrdinalIgnoreCase))
                return Unauthorized(ApiResponse.Fail(genericFailure));

            string newHash = _pwd.Hash(req.NewPassword);
            string etag    = await _bc.GetEtagAsync(req.EmployeeNo);
            await _bc.UpdatePasswordHashAsync(req.EmployeeNo, newHash, etag);

            return Ok(ApiResponse.Ok("Password reset successfully. You can now sign in with your new password."));
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
            EmployeeNo     = employeeNo,
            FirstName      = GetString(root, "First_Name"),
            LastName       = GetString(root, "Last_Name"),
            Email          = GetString(root, "Company_E_Mail"),
            JobTitle       = GetString(root, "Job_Title"),
            PhoneNo        = GetString(root, "Phone_No"),
            Gender         = GetString(root, "Gender"),
            EmploymentType = GetString(root, "Engagement_Type"),
            Status         = GetString(root, "Status"),
        };

    private static string GetString(System.Text.Json.JsonElement root, string key) =>
        root.TryGetProperty(key, out var prop) ? prop.GetString() ?? "" : "";
}