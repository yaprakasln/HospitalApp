using Microsoft.EntityFrameworkCore;
using YeniHospitalAPI.Data;
using YeniHospitalAPI.Models;

namespace YeniHospitalAPI.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task<Patient?> UpdatePatientAsync(int id, Patient patient);
        Task<bool> DeletePatientAsync(int id);
    }

    public class PatientService : IPatientService
    {
        private readonly HospitalDbContext _context;

        public PatientService(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients.ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _context.Patients.FindAsync(id);
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<Patient?> UpdatePatientAsync(int id, Patient patient)
        {
            if (id != patient.Id)
                return null;

            _context.Entry(patient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return patient;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PatientExistsAsync(id))
                    return null;
                throw;
            }
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> PatientExistsAsync(int id)
        {
            return await _context.Patients.AnyAsync(e => e.Id == id);
        }
    }
}
