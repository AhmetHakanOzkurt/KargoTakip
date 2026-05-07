using BCrypt.Net;
using FluentValidation;
using KargoTakip.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly KargoTakipDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IValidator<LoginRequest> _validator;

        public AuthController(
            KargoTakipDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IValidator<LoginRequest> validator)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _validator = validator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors
                    .Select(e => new { field = e.PropertyName, message = e.ErrorMessage }));
            _logger.LogInformation("Login denemesi: {Username}", request.Username);

            var user = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Başarısız login: {Username} bulunamadı", request.Username);
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." });
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Başarısız login: {Username} şifre hatalı", request.Username);
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." });
            }

            _logger.LogInformation("Başarılı login: {Username}, Şube: {Branch}",
                user.Username, user.Branch.Name);

            var token = GenerateJwtToken(user.Id, user.Username, user.Role, user.BranchId);

            return Ok(new
            {
                token,
                username = user.Username,
                fullName = user.FullName,
                role = user.Role,
                branchId = user.BranchId,
                branchName = user.Branch.Name
            });
        }

        private string GenerateJwtToken(int userId, string username, string role, int branchId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("username", username),
                new Claim(ClaimTypes.Role, role),
                new Claim("branchId", branchId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    double.Parse(jwtSettings["ExpiryInHours"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
       
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}
