using System;
using System.Collections.Generic;
using System.Text;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;

namespace HealthcareBooking.Core.Services;

public class DoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DoctorService(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> AddDoctorAsync(Doctor doctor)
    {
        await _doctorRepository.AddAsync(doctor);
        await _unitOfWork.SaveChangesAsync();
        return doctor.Id;
    }

    public Task<IReadOnlyList<Doctor>> GetAllDoctorsAsync()
    {
        return _doctorRepository.GetAllAsync();
    }

    public async Task<Doctor> GetDoctorByIdAsync(int doctorId)
    {
        return await _doctorRepository.GetByIdAsync(doctorId)
            ?? throw new KeyNotFoundException("醫生不存在");
    }

    public async Task UpdateDoctorAsync(int doctorId, string name)
    {
        var doctor = await GetDoctorByIdAsync(doctorId);
        doctor.UpdateName(name);
        await _doctorRepository.UpdateAsync(doctor);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteDoctorAsync(int doctorId)
    {
        _ = await GetDoctorByIdAsync(doctorId);
        await _doctorRepository.DeleteAsync(doctorId);
        await _unitOfWork.SaveChangesAsync();
    }
}
