using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;

namespace HealthcareBooking.Core.Services;

public class AppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(IAppointmentRepository appointmentRepository, IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> AddAppointmentAsync(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);

        await _appointmentRepository.AddAsync(appointment);
        await _unitOfWork.SaveChangesAsync();
        return appointment.Id;
    }

    public Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync()
    {
        return _appointmentRepository.GetAllAsync();
    }

    public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
    {
        return await _appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("預約不存在");
    }

    public async Task UpdateAppointmentAsync(int appointmentId, Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);

        var existingAppointment = await GetAppointmentByIdAsync(appointmentId);
        existingAppointment.PatientId = appointment.PatientId;
        existingAppointment.ClinicId = appointment.ClinicId;
        existingAppointment.AppointmentDate = appointment.AppointmentDate;

        await _appointmentRepository.UpdateAsync(existingAppointment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAppointmentAsync(int appointmentId)
    {
        _ = await GetAppointmentByIdAsync(appointmentId);
        await _appointmentRepository.DeleteAsync(appointmentId);
        await _unitOfWork.SaveChangesAsync();
    }
}
