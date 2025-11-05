using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YeniHospitalAPI.Models;
using YeniHospitalAPI.Services;

namespace YeniHospitalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetPatients([FromQuery] string? token = null)
        {
            var patients = await _patientService.GetAllPatientsAsync();
            
            return Ok(new {
                message = "Hasta listesi (Service katmanından)",
                totalPatients = patients.Count(),
                patients = patients,
                note = "Bu veriler Service katmanından geliyor"
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPatient(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);

            if (patient == null)
            {
                return NotFound(new { message = "Hasta bulunamadı", patientId = id });
            }

            return Ok(new {
                message = "Hasta detayı (Service katmanından)",
                patient = patient
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> PostPatient(Patient patient)
        {
            var createdPatient = await _patientService.CreatePatientAsync(patient);

            return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.Id }, new {
                message = "Hasta eklendi (Service katmanından)",
                patient = createdPatient
            });
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> PutPatient(int id, Patient patient)
        {
            var updatedPatient = await _patientService.UpdatePatientAsync(id, patient);

            if (updatedPatient == null)
            {
                return BadRequest(new { message = "Güncelleme başarısız" });
            }

            return Ok(new { 
                message = "Hasta güncellendi (Service katmanından)", 
                patient = updatedPatient 
            });
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var result = await _patientService.DeletePatientAsync(id);
            
            if (!result)
            {
                return NotFound(new { message = "Hasta bulunamadı" });
            }

            return Ok(new { message = "Hasta silindi (Service katmanından)" });
        }
    }
}
