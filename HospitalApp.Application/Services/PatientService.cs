using Microsoft.EntityFrameworkCore;
using HospitalApp.Domain.Entities;

namespace HospitalApp.Application.Services
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
        private readonly DbContext _context;

        public PatientService(DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _context.Set<Patient>().ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _context.Set<Patient>().FindAsync(id);
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            _context.Set<Patient>().Add(patient);
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
            var patient = await _context.Set<Patient>().FindAsync(id);
            if (patient == null)
                return false;

            _context.Set<Patient>().Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> PatientExistsAsync(int id)
        {
            return await _context.Set<Patient>().AnyAsync(e => e.Id == id);
        }
    }
}