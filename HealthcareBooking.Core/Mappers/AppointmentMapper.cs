using HealthcareBooking.Core.DTOs;
using HealthcareBooking.Core.Entities;

namespace HealthcareBooking.Core.Mappers;

public static class AppointmentMapper
{
    public static AppointmentDetailDto ToDetailDto(Appointment appointment) => new(
        appointment.Id,
        appointment.AppointmentDate,
        appointment.Clinic.ClinicDate.ToString("yyyy-MM-dd"),
        appointment.Clinic.Doctor.Name
    );
}
