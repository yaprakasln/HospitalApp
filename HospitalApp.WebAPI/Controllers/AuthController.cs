using Microsoft.AspNetCore.Mvc;
using HospitalApp.Application.DTOs;
using HospitalApp.Application.Services;
using HospitalApp.WebAPI.Services;

namespace HospitalApp.WebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpGet("register")]
        public async Task<ActionResult> GetRegisterPage([FromQuery] string? username = null, [FromQuery] string? email = null, [FromQuery] string? password = null)
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
                        Password = password
                    };

                    var result = await _authService.RegisterAsync(registerDto);
                    
                    // Get user from database to generate token
                    var dbUser = await _authService.GetUserByUsernameAsync(result.Username);
                    var token = _jwtService.GenerateToken(dbUser);

                    return Ok(new {
                        success = true,
                        message = $"Kayıt başarılı! Hoş geldin {result.Username}!",
                        registrationTime = DateTime.Now,
                        user = new {
                            username = result.Username
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
                        providedData = new { username, email }
                    });
                }
            }

            // Parametreler yoksa kayıt sayfası bilgilerini göster + mevcut kullanıcıları listele
            var users = await _authService.GetAllUsersAsync();
            
            return Ok(new { 
                currentUsers = new {
                    total = users.Count(),
                    users = users
                }
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                
                // Get user from database to generate token
                var dbUser = await _authService.GetUserByUsernameAsync(result.Username);
                var token = _jwtService.GenerateToken(dbUser);
                
                result.Token = token;
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("login")]
        public async Task<ActionResult> GetLoginPage([FromQuery] string? username = null, [FromQuery] string? password = null)
        {
            // Eğer parametreler varsa giriş yap
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    var loginDto = new LoginDto
                    {
                        Username = username,
                        Password = password
                    };

                    var result = await _authService.LoginAsync(loginDto);
                    
                    // Get user from database to generate token
                    var dbUser = await _authService.GetUserByUsernameAsync(result.Username);
                    var token = _jwtService.GenerateToken(dbUser);

                    return Ok(new {
                        success = true,
                        message = $"Giriş başarılı! Hoş geldin {result.Username}!",
                        loginTime = DateTime.Now,
                        user = new {
                            username = result.Username
                        },
                        token = token,
                        nextSteps = new {
                            patientsUrl = $"/api/patients?token={token}"
                        }
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { 
                        success = false,
                        message = ex.Message,
                        providedData = new { username }
                    });
                }
            }

            // Parametreler yoksa giriş sayfası bilgilerini göster
            return Ok(new { 
                message = "Giriş yapmak için username ve password parametrelerini ekleyin",
                registerUrl = "/api/auth/register"
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                
                // Get user from database to generate token
                var dbUser = await _authService.GetUserByUsernameAsync(result.Username);
                var token = _jwtService.GenerateToken(dbUser);
                
                result.Token = token;
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }


    }
}