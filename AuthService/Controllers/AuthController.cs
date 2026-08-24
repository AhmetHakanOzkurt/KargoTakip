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

        [HttpPost("users")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
                return Forbid();

            var exists = await _context.Users.AnyAsync(u => u.Username == request.Username);
            if (exists)
                return BadRequest(new { message = "Bu kullanıcı adı zaten kullanılıyor." });

            var branch = await _context.Branches.FindAsync(request.BranchId);
            if (branch == null)
                return BadRequest(new { message = "Şube bulunamadı." });

            var user = new KargoTakip.Infrastructure.Models.User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Role = request.Role,
                BranchId = request.BranchId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Yeni kullanıcı oluşturuldu: {Username}", user.Username);

            return Ok(new { user.Id, user.Username, user.FullName, user.Role, message = "Kullanıcı oluşturuldu." });
        }

        [HttpGet("branches")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _context.Branches
                .Where(b => b.IsActive)
                .Select(b => new { b.Id, b.Name })
                .OrderBy(b => b.Name)
                .ToListAsync();
            return Ok(branches);
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
                userId = user.Id,
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

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int BranchId { get; set; }
    }

}
