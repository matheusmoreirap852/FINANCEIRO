using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MeuProjetoFinanceiro.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var username = _configuration["Auth:Username"] ?? "admin";
        var password = _configuration["Auth:Password"] ?? "admin";

        if (!SafeEquals(request.Username, username) || !SafeEquals(request.Password, password))
        {
            return Unauthorized(new { message = "Usuario ou senha invalidos." });
        }

        var expiresAt = DateTime.UtcNow.AddHours(12);
        var token = CreateToken(username, expiresAt);

        return Ok(new LoginResponse(token, "Bearer", expiresAt, username));
    }

    private string CreateToken(string username, DateTime expiresAt)
    {
        var jwtKey = _configuration["Auth:JwtKey"]
            ?? "dev-only-change-this-key-with-at-least-32-characters";
        var issuer = _configuration["Auth:Issuer"] ?? "MeuProjetoFinanceiro";
        var audience = _configuration["Auth:Audience"] ?? "MeuProjetoFinanceiro.App";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Name, username),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool SafeEquals(string? value, string expected)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return valueBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(valueBytes, expectedBytes);
    }
}

public record LoginRequest(string Username, string Password);

public record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt, string Username);
