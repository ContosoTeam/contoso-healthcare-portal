using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContosoHealthcare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // VULNERABILITY: Hardcoded credentials
        private readonly Dictionary<string, string> _users = new()
        {
            { "doctor.smith", "Welcome1!" },
            { "nurse.jones", "Password123" },
            { "admin", "Admin@2024" },
            { "lab_tech", "LabT3ch!" }
        };

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // VULNERABILITY: No brute-force protection, no account lockout
            if (_users.TryGetValue(request.Username, out string? password) && password == request.Password)
            {
                var token = GenerateToken(request.Username);
                return Ok(new { token, expiresIn = "365d" });
            }
            
            return Unauthorized(new { error = "Invalid credentials" });
        }

        private string GenerateToken(string username)
        {
            var secret = _config["Jwt:Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
            
            // VULNERABILITY: Using HS256 with weak key
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "admin"), // VULNERABILITY: Everyone gets admin role
                new Claim("facility", "all")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(365), // VULNERABILITY: 1-year token expiration
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
