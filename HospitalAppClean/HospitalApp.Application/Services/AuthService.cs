using Microsoft.EntityFrameworkCore;
using HospitalApp.Application.DTOs;
using HospitalApp.Domain.Entities;

namespace HospitalApp.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<IEnumerable<object>> GetAllUsersAsync();
        Task<User> GetUserByUsernameAsync(string username);
    }

    public class AuthService : IAuthService
    {
        private readonly DbContext _context;

        public AuthService(DbContext context)
        {
            _context = context;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Set<User>().AnyAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email))
            {
                throw new ArgumentException("Kullanıcı adı veya email zaten kullanımda");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = "TOKEN_PLACEHOLDER", // JWT service'den gelecek
                Username = user.Username,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Username veya email ile giriş yapabilir
            var user = await _context.Set<User>().FirstOrDefaultAsync(u => 
                (u.Username == loginDto.Username || u.Email == loginDto.Username) && u.IsActive);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Kullanıcı bulunamadı");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Şifre yanlış");
            }

            return new AuthResponseDto
            {
                Token = "TOKEN_PLACEHOLDER", // JWT service'den gelecek
                Username = user.Username,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            return await _context.Set<User>()
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.Username == username && u.IsActive) 
                ?? throw new ArgumentException("Kullanıcı bulunamadı");
        }
    }
}