using HealthcareBooking.Core.DTOs;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;

namespace HealthcareBooking.Core.Services;

public class PatientQueryService
{
    private readonly IPatientRepository _patientRepository;

    public PatientQueryService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    // 這裡回傳的 DTO (PatientHistoryDto) 會在 API 層或 Core 層定義
    public async Task<PatientHistoryDto> GetPatientHistoryAsync(int patientId)
    {
        var patient = await _patientRepository.GetPatientWithHistoryAsync(patientId);

        if (patient == null)
        {
            throw new KeyNotFoundException("查無此病患");
        }

        // 將 patient 實體轉換為 DTO 並回傳
        // (例如手動 mapping 或是使用 AutoMapper)
        return MapToDto(patient);
    }

    private static PatientHistoryDto MapToDto(Patient patient)
    {
        return new PatientHistoryDto(
            patient.Id,
            patient.Name,
            patient.Appointments.Select(a => new AppointmentDetailDto(
                a.Id,
                a.AppointmentDate,
                a.Clinic.ClinicDate.ToString("yyyy-MM-dd"),
                a.Clinic.Doctor.Name
            )).ToList()
        );
    }
}