using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.Infrastructure.Repositories;

public class ClinicRepository : IClinicRepository
{
    private readonly AppDbContext _context;

    public ClinicRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Clinic clinic)
    {
        await _context.Clinics.AddAsync(clinic);
    }

    public async Task DeleteAsync(int clinicId)
    {
        var clinic = await _context.Clinics.FindAsync(clinicId);
        if (clinic != null)
        {
            _context.Clinics.Remove(clinic);
        }
    }

    public async Task<IReadOnlyList<Clinic>> GetAllAsync()
    {
        return await _context.Clinics.ToListAsync();
    }

    public async Task<Clinic?> GetByIdAsync(int clinicId)
    {
        Clinic? clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId);
        return clinic;
    }

    public Task UpdateAsync(Clinic clinc)
    {
        _context.Clinics.Update(clinc);
        return Task.CompletedTask;
    }
}
