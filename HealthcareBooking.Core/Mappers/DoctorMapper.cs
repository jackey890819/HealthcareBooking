using HealthcareBooking.Core.DTOs;
using HealthcareBooking.Core.Entities;

namespace HealthcareBooking.Core.Mappers;

public static class DoctorMapper
{
    public static DoctorDto ToDto(Doctor doctor) => new(
        doctor.Id,
        doctor.Name
    );

    public static IReadOnlyList<DoctorDto> ToDtoList(IEnumerable<Doctor> doctors) =>
        doctors.Select(ToDto).ToList().AsReadOnly();
}
