using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;
    public PatientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
    }

    public async Task DeleteAsync(int patientId)
    {
        Patient? patient = await _context.Patients.FindAsync(patientId);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
        }
    }

    public async Task<IReadOnlyList<Patient>> GetAllAsync()
    {
        return await _context.Patients.ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(int patientId)
    {
        return await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
    }

    public Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        return Task.CompletedTask;
    }
}
