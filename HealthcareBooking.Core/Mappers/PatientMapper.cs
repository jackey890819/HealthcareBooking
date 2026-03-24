using HealthcareBooking.Core.DTOs;
using HealthcareBooking.Core.Entities;

namespace HealthcareBooking.Core.Mappers;

public static class PatientMapper
{
    public static PatientHistoryDto ToHistoryDto(Patient patient) => new(
        patient.Id,
        patient.Name,
        patient.Appointments.Select(AppointmentMapper.ToDetailDto).ToList()
    );
}
