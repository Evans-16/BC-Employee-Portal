namespace EmployeePortal.Models;

// ─── Inbound Requests ────────────────────────────────────────────────────────

public class LoginRequest
{
    public string EmployeeNo { get; set; } = string.Empty;
    public string Password   { get; set; } = string.Empty;
}

// ─── Outbound Responses ──────────────────────────────────────────────────────

public class LoginResponse
{
    public string Message     { get; set; } = string.Empty;
    public string EmployeeNo  { get; set; } = string.Empty;
    public string FirstName   { get; set; } = string.Empty;
    public string Email       { get; set; } = string.Empty;
}