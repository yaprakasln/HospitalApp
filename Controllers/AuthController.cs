using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YeniHospitalAPI.Data;
using YeniHospitalAPI.DTOs;
using YeniHospitalAPI.Models;

namespace YeniHospitalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(HospitalDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("register")]
        public async Task<ActionResult> GetRegisterPage([FromQuery] string? username = null, [FromQuery] string? email = null, [FromQuery] string? password = null, [FromQuery] string? role = null)
        {
            // Eğer parametreler varsa kayıt yap
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    var registerDto = new RegisterDto
                    {
                        Username = username,
                        Email = email,
                        Password = password,
                        Role = role ?? "Patient"
                    };

                    // Kullanıcı zaten var mı kontrol et
                    if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
                    {
                        return BadRequest(new { 
                            success = false,
                            message = "Kullanıcı adı veya email zaten kullanımda",
                            providedData = new { username, email, role }
                        });
                    }

                    // Şifreyi hash'le
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

                    // Yeni kullanıcı oluştur
                    var user = new User
                    {
                        Username = registerDto.Username,
                        Email = registerDto.Email,
                        PasswordHash = passwordHash,
                        Role = registerDto.Role,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Token oluştur
                    var token = GenerateJwtToken(user);

                    return Ok(new {
                        success = true,
                        message = $"Kayıt başarılı! Hoş geldin {user.Username}!",
                        registrationTime = DateTime.Now,
                        user = new {
                            username = user.Username,
                            email = user.Email,
                            role = user.Role
                        },
                        token = token,
                        nextSteps = new {
                            loginUrl = $"/api/auth/login?token={token}",
                            patientsUrl = $"/api/patients?token={token}"
                        }
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { 
                        success = false,
                        message = ex.Message,
                        providedData = new { username, email, role }
                    });
                }
            }

            // Parametreler yoksa kayıt sayfası bilgilerini göster + mevcut kullanıcıları listele
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .ToListAsync();
            
            return Ok(new { 

                
                currentUsers = new {
                    total = users.Count,
                    users = users
                },
                availableRoles = new[] { "Doctor","Patient" }
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            // Kullanıcı zaten var mı kontrol et
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
            {
                return BadRequest("Kullanıcı adı veya email zaten kullanımda");
            }

            // Şifreyi hash'le
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Yeni kullanıcı oluştur
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Token oluştur
            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            // Kullanıcıyı bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Geçersiz kullanıcı adı veya şifre");
            }

            // Token oluştur
            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
