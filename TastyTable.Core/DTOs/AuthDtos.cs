using System.ComponentModel.DataAnnotations;

namespace TastyTable.Core.DTOs;

// Use `required` so the compiler knows these must be set by model binding.
// This removes CS8618 while still showing empty inputs in Swagger (no defaults).
public class LoginRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class RegisterRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }

    // Email is REQUIRED and non-nullable
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}

// Response DTO used by AuthController
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
