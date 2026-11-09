using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration? _configuration;

        public AuthController(AppDbContext context, IConfiguration? configuration = null)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var normalizedEmail = dto.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            {
                return BadRequest(new { message = "An account with this email already exists." });
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password.Trim());

            var user = new User
            {
                FullName = string.IsNullOrWhiteSpace(dto.FullName) ? "RaceDay User" : dto.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = hashedPassword,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Participant" : dto.Role.Trim()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful. You can now log in." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var normalizedEmail = dto.Email.Trim().ToLower();
            var inputPassword = dto.Password.Trim();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            bool isValid = false;
            try
            {
                isValid = BCrypt.Net.BCrypt.Verify(inputPassword, user.PasswordHash);
            }
            catch
            {
                isValid = false;
            }

            if (!isValid && user.PasswordHash == inputPassword)
            {
                isValid = true;
            }

            if (!isValid)
            {
                return BadRequest(new { message = "Invalid email or password." });
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                role = user.Role,
                email = user.Email,
                name = user.FullName,
                userId = user.UserId
            });
        }

        private string GenerateJwtToken(User user)
        {
            var keyString = _configuration?["Jwt:Key"] ?? "RaceDaySuperSecretKeyForJwtAuthentication2026!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "Participant")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration?["Jwt:Issuer"] ?? "RaceDayApi",
                audience: _configuration?["Jwt:Audience"] ?? "RaceDayWeb",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}