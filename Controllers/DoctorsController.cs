using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YeniHospitalAPI.Data;

namespace YeniHospitalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public DoctorsController(HospitalDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        [AllowAnonymous] // Token olmadan da erişilebilir
        public async Task<ActionResult> GetDoctorDashboard([FromQuery] string? token = null)
        {
            var patients = await _context.Patients.ToListAsync();
            
            // Eğer token varsa kullanıcı bilgilerini göster
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // JWT token'ı decode et
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    
                    var username = jsonToken.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
                    var role = jsonToken.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                    
                    if (role == "Doctor")
                    {
                        return Ok(new {
                            message = $"Doktor Paneli - Dr. {username}",
                            totalPatients = patients.Count,
                            patients = patients,
                            doctor = new {
                                username = username,
                                role = role,
                                timestamp = DateTime.Now
                            },
                            permissions = new[] { "Hasta görüntüleme", "Hasta ekleme", "Hasta güncelleme" }
                        });
                    }
                    
                }
                catch
                {
                    return BadRequest(new { message = "Geçersiz token" });
                }
            }
            
            // Header'dan token kontrol et
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUser = User.FindFirst(ClaimTypes.Name)?.Value;
                var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (currentRole == "Doctor")
                {
                    return Ok(new {
                        message = $"Doktor Paneli - Dr. {currentUser}",
                        totalPatients = patients.Count,
                        patients = patients,
                        doctor = new {
                            username = currentUser,
                            role = currentRole,
                            timestamp = DateTime.Now
                        },
                        permissions = new[] { "Hasta görüntüleme", "Hasta ekleme", "Hasta güncelleme" }
                    });
                }
                else
                {
                    return Forbid("Bu panel sadece doktorlar için");
                }
            }
            
            // Token yoksa giriş bilgileri göster
            return Ok(new {
                message = "Doktor Paneli - Giriş Gerekli",
                instruction = "Doktor olarak giriş yapın",
                loginSteps = new {
                    step1 = "POST /api/auth/register ile Doctor rolü ile kayıt ol",
                    step2 = "POST /api/auth/login ile giriş yap",
                    step3 = "Token'ı al ve /api/doctors/dashboard?token=YOUR_TOKEN ile eriş"
                },
                registerExample = "/api/auth/register?username=doktor1&email=doktor1@hospital.com&password=Doktor123!&role=Doctor"
            });
        }

        [HttpGet("info")]
        [AllowAnonymous]
        public async Task<ActionResult> GetDoctorsInfo()
        {
            var doctors = await _context.Users
                .Where(u => u.Role == "Doctor" && u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(new {
                message = "Doktor Listesi",
                totalDoctors = doctors.Count,
                doctors = doctors,
                note = "Doktor paneline erişmek için giriş yapın"
            });
        }
    }
}
