using System;
using System.Collections.Generic;
using System.Text;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Interfaces;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Core.DTOs;
using HealthcareBooking.Core.Mappers;

namespace HealthcareBooking.Core.Services;

public class DoctorService
{
    private const string AllDoctorsCacheKey = "doctors:all";
    private static string DoctorCacheKey(int id) => $"doctor:{id}";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DoctorService(IDoctorRepository doctorRepository, IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<int> AddDoctorAsync(Doctor doctor)
    {
        await _doctorRepository.AddAsync(doctor);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveAsync(AllDoctorsCacheKey);
        return doctor.Id;
    }

    public async Task<IReadOnlyList<DoctorDto>> GetAllDoctorsAsync()
    {
        var cached = await _cacheService.GetAsync<IReadOnlyList<DoctorDto>>(AllDoctorsCacheKey);
        if (cached is not null)
            return cached;

        var doctors = await _doctorRepository.GetAllAsync();
        var dtos = DoctorMapper.ToDtoList(doctors);
        await _cacheService.SetAsync(AllDoctorsCacheKey, dtos, CacheExpiration);
        return dtos;
    }

    public async Task<DoctorDto> GetDoctorByIdAsync(int doctorId)
    {
        var cacheKey = DoctorCacheKey(doctorId);
        var cached = await _cacheService.GetAsync<DoctorDto>(cacheKey);
        if (cached is not null)
            return cached;

        var doctor = await _doctorRepository.GetByIdAsync(doctorId)
            ?? throw new KeyNotFoundException("醫生不存在");

        var dto = DoctorMapper.ToDto(doctor);
        await _cacheService.SetAsync(cacheKey, dto, CacheExpiration);
        return dto;
    }

    private async Task<Doctor> GetDoctorEntityByIdAsync(int doctorId)
    {
        return await _doctorRepository.GetByIdAsync(doctorId)
            ?? throw new KeyNotFoundException("醫生不存在");
    }

    public async Task UpdateDoctorAsync(int doctorId, string name)
    {
        var doctor = await GetDoctorEntityByIdAsync(doctorId);
        doctor.UpdateName(name);
        await _doctorRepository.UpdateAsync(doctor);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveAsync(DoctorCacheKey(doctorId));
        await _cacheService.RemoveAsync(AllDoctorsCacheKey);
    }

    public async Task DeleteDoctorAsync(int doctorId)
    {
        _ = await GetDoctorEntityByIdAsync(doctorId);
        await _doctorRepository.DeleteAsync(doctorId);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveAsync(DoctorCacheKey(doctorId));
        await _cacheService.RemoveAsync(AllDoctorsCacheKey);
    }
}
