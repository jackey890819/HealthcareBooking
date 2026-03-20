using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _context;

        public DoctorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Doctor doctor)
        {
            await _context.Doctors.AddAsync(doctor);
        }

        public async Task DeleteAsync(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }
        }

        public async Task<IReadOnlyList<Doctor>> GetAllAsync()
        {
            return await _context.Doctors.ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int doctorId)
        {
            Doctor? doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            return doctor;
        }

        public Task UpdateAsync(Doctor doctor)
        {
            _context.Doctors.Update(doctor);
            return Task.CompletedTask;
        }
    }
}
