using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;

namespace HealthcareBooking.Core.Services;

public class PatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PatientService(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> AddPatientAsync(Patient patient)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentException.ThrowIfNullOrWhiteSpace(patient.Name);

        await _patientRepository.AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();
        return patient.Id;
    }

    public Task<IReadOnlyList<Patient>> GetAllPatientsAsync()
    {
        return _patientRepository.GetAllAsync();
    }

    public async Task<Patient> GetPatientByIdAsync(int patientId)
    {
        return await _patientRepository.GetByIdAsync(patientId)
            ?? throw new KeyNotFoundException("病患不存在");
    }

    public async Task UpdatePatientAsync(int patientId, Patient patient)
    {
        ArgumentNullException.ThrowIfNull(patient);
        ArgumentException.ThrowIfNullOrWhiteSpace(patient.Name);

        var existingPatient = await GetPatientByIdAsync(patientId);
        existingPatient.Name = patient.Name;

        await _patientRepository.UpdateAsync(existingPatient);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeletePatientAsync(int patientId)
    {
        _ = await GetPatientByIdAsync(patientId);
        await _patientRepository.DeleteAsync(patientId);
        await _unitOfWork.SaveChangesAsync();
    }
}
