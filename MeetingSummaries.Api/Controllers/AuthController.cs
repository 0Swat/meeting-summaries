using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MeetingSummaries.Api.Data;
using MeetingSummaries.Api.Dto.Requests;
using MeetingSummaries.Api.Dto.Responses;
using MeetingSummaries.Api.Models;
using MeetingSummaries.Api.Services;

namespace MeetingSummaries.Api.Controllers;

/// <summary>
/// Uwierzytelnianie i rejestracja użytkowników.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase
{
    /// <summary>
    /// Rejestracja nowego użytkownika.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Login i hasło są wymagane.");

        if (await db.Users.AnyAsync(u => u.Username == request.Username))
            return Conflict("Użytkownik o tej nazwie już istnieje.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username.Trim(),
            PasswordHash = PasswordHelper.HashPassword(request.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(BuildToken(user));
    }

    /// <summary>
    /// Logowanie — zwraca token JWT ważny przez skonfigurowaną liczbę dni.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Nieprawidłowa nazwa użytkownika lub hasło.");

        return Ok(BuildToken(user));
    }

    /// <summary>
    /// Zmiana hasła zalogowanego użytkownika.
    /// </summary>
    [Authorize]
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user is null) return Unauthorized();

        if (!PasswordHelper.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return BadRequest("Aktualne hasło jest nieprawidłowe.");

        if (request.NewPassword.Length < 4)
            return BadRequest("Nowe hasło musi mieć co najmniej 4 znaki.");

        user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Usunięcie konta wraz ze wszystkimi danymi.
    /// </summary>
    [Authorize]
    [HttpDelete("account")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(userId);
        if (user is null) return Unauthorized();

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private LoginResponse BuildToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryDays = int.TryParse(config["Jwt:ExpiryDays"], out var d) ? d : 30;
        var expires = DateTime.UtcNow.AddDays(expiryDays);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())],
            expires: expires,
            signingCredentials: creds
        );

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
