namespace EmployeePortal.Models;

// ─── Generic API response wrapper ────────────────────────────────────────────

public class ApiResponse<T>
{
    public bool   Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T?     Data    { get; set; }

    public static ApiResponse<T> Ok(string message, T? data = default) =>
        new() { Success = true,  Message = message, Data = data };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message)   => new() { Success = true,  Message = message };
    public static ApiResponse Fail(string message) => new() { Success = false, Message = message };
}

// ─── Inbound request DTOs ────────────────────────────────────────────────────

public class SignupRequest
{
    public string EmployeeNo { get; set; } = string.Empty;
    public string Password   { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string EmployeeNo { get; set; } = string.Empty;
    public string Password   { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string EmployeeNo      { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword     { get; set; } = string.Empty;
}

/// <summary>
/// Used for "forgot password" — verifies identity via Employee Number +
/// National ID (BC's Social_Security_No field) instead of requiring the
/// current password, since the user doesn't have it.
/// </summary>
public class ForgotPasswordRequest
{
    public string EmployeeNo  { get; set; } = string.Empty;
    public string NationalId  { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateEmployeeRequest
{
    // Only non-null fields are sent to BC — omitted fields are untouched
    public string? FirstName      { get; set; }
    public string? LastName       { get; set; }
    public string? CompanyEmail   { get; set; }
    public string? PhoneNo        { get; set; }
    public string? JobTitle       { get; set; }
    public string? Gender         { get; set; }
    public string? EmploymentType { get; set; } // maps to BC "Engagement Type"
}

// ─── Outbound response DTOs ──────────────────────────────────────────────────

public class AuthData
{
    public string EmployeeNo     { get; set; } = string.Empty;
    public string FirstName      { get; set; } = string.Empty;
    public string LastName       { get; set; } = string.Empty;
    public string Email          { get; set; } = string.Empty;
    public string JobTitle       { get; set; } = string.Empty;
    public string PhoneNo        { get; set; } = string.Empty;
    public string Gender         { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Status         { get; set; } = string.Empty;
}

public class EmployeeData
{
    public string EmployeeNo     { get; set; } = string.Empty;
    public string FirstName      { get; set; } = string.Empty;
    public string LastName       { get; set; } = string.Empty;
    public string Email          { get; set; } = string.Empty;
    public string JobTitle       { get; set; } = string.Empty;
    public string PhoneNo        { get; set; } = string.Empty;
    public string Gender         { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Status         { get; set; } = string.Empty;
    public bool   IsActive       { get; set; }
}