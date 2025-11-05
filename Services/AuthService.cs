using Microsoft.EntityFrameworkCore;
using YeniHospitalAPI.Data;
using YeniHospitalAPI.DTOs;
using YeniHospitalAPI.Models;

namespace YeniHospitalAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<IEnumerable<object>> GetAllUsersAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly HospitalDbContext _context;

        public AuthService(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
            {
                throw new ArgumentException("Kullanıcı adı veya email zaten kullanımda");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

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

            return new AuthResponseDto
            {
                Token = "TOKEN_PLACEHOLDER", // JWT service'den gelecek
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Geçersiz kullanıcı adı veya şifre");
            }

            return new AuthResponseDto
            {
                Token = "TOKEN_PLACEHOLDER", // JWT service'den gelecek
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            return await _context.Users
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
        }
    }
}
