using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;

namespace HealthcareBooking.Core.Services;

public class ClinicService
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClinicService(IClinicRepository clinicRepository, IUnitOfWork unitOfWork)
    {
        _clinicRepository = clinicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> AddClinicAsync(Clinic clinic)
    {
        ArgumentNullException.ThrowIfNull(clinic);

        await _clinicRepository.AddAsync(clinic);
        await _unitOfWork.SaveChangesAsync();
        return clinic.Id;
    }

    public Task<IReadOnlyList<Clinic>> GetAllClinicsAsync()
    {
        return _clinicRepository.GetAllAsync();
    }

    public async Task<Clinic> GetClinicByIdAsync(int clinicId)
    {
        return await _clinicRepository.GetByIdAsync(clinicId)
            ?? throw new KeyNotFoundException("門診不存在");
    }

    public async Task UpdateClinicAsync(int clinicId, int doctorId, DateTime clinicDate, int maxQuota)
    {
        var existingClinic = await GetClinicByIdAsync(clinicId);
        existingClinic.UpdateDetails(doctorId, clinicDate, maxQuota);

        await _clinicRepository.UpdateAsync(existingClinic);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteClinicAsync(int clinicId)
    {
        _ = await GetClinicByIdAsync(clinicId);
        await _clinicRepository.DeleteAsync(clinicId);
        await _unitOfWork.SaveChangesAsync();
    }
}
