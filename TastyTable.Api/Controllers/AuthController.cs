using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TastyTable.Core.DTOs;
using TastyTable.Core.Interfaces;
using TastyTable.Services;

namespace TastyTable.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly JwtTokenService _jwt;

    public AuthController(IUserService users, JwtTokenService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    /// <summary>
    /// Login with username and password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] LoginRequest request)
    {
        var user = await _users.ValidateUserAsync(request.Username, request.Password);
        if (user == null) return Unauthorized();

        var token = _jwt.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role
        });
    }


    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
        var existing = await _users.GetByUsernameAsync(request.Username);
        if (existing != null)
            return BadRequest(new { message = "Username already taken" });

        var user = await _users.RegisterAsync(request.Username, request.Password, request.Email);
        return Ok(user);
    }
}