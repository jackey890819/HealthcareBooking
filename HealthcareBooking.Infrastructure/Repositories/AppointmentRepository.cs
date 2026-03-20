using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
    }

    public async Task DeleteAsync(int appointmentId)
    {
        Appointment? appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
        }
    }

    public async Task<IReadOnlyList<Appointment>> GetAllAsync()
    {
        var appointments = await _context.Appointments.ToListAsync();
        return appointments;
    }

    public async Task<Appointment?> GetByIdAsync(int appointmentId)
    {
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
        return appointment;
    }

    public Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        return Task.CompletedTask;
    }
}
