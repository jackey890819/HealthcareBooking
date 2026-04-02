using System;
using System.Collections.Concurrent;
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
    private static TimeSpan CacheExpiration => TimeSpan.FromSeconds(600 + new Random().Next(1, 300));
    private static TimeSpan CacheNullExpriation => TimeSpan.FromSeconds(30 + new Random().Next(1, 10));
    // 用於控制同一時間只有一個執行緒可以進行快取重建，避免大量請求同時落到資料庫上
    private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    // 每位醫生獨立的 Semaphore，避免不同醫生的請求互相阻塞
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _doctorSemaphores = new();

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
        // 第一次快取檢查
        var cached = await _cacheService.GetAsync<IReadOnlyList<DoctorDto>>(AllDoctorsCacheKey);
        if (cached is not null)
            return cached;

        // 快取沒有命中
        await semaphore.WaitAsync();
        try
        {
            // 第二次快取檢查，避免在等待鎖的過程中已經有其他執行緒重建了快取
            cached = await _cacheService.GetAsync<IReadOnlyList<DoctorDto>>(AllDoctorsCacheKey);
            if (cached is not null)
                return cached;

            // 確定快取沒有命中，從資料庫讀取資料
            var doctors = await _doctorRepository.GetAllAsync();
            var doctorDtos = DoctorMapper.ToDtoList(doctors);

            // 將資料寫入快取，並設定過期時間
            await _cacheService.SetAsync(AllDoctorsCacheKey, doctorDtos, CacheExpiration);

            return doctorDtos;
        }
        finally
        {
            // 釋放鎖，讓其他等待的執行緒可以繼續執行
            // 注意：即使在重建快取的過程中發生例外，也要確保鎖能夠被釋放，避免死鎖
            semaphore.Release();
        }
    }

    public async Task<DoctorDto> GetDoctorByIdAsync(int doctorId)
    {
        var cacheKey = DoctorCacheKey(doctorId);
        // 第一次快取檢查
        // CacheEntry 為 null       → 快取未命中，需查資料庫
        // CacheEntry.Value 有值    → 快取命中，醫生存在
        // CacheEntry.Value 為 null → 快取命中，醫生不存在（空值快取，防止快取穿透）
        var cached = await _cacheService.GetAsync<CacheEntry<DoctorDto>>(cacheKey);
        if (cached is not null)
            return cached.Value ?? throw new KeyNotFoundException("醫生不存在");

        // 快取沒有命中，取得此醫生專屬的 Semaphore（防止快取擊穿）
        var doctorSemaphore = _doctorSemaphores.GetOrAdd(doctorId, _ => new SemaphoreSlim(1, 1));
        await doctorSemaphore.WaitAsync();
        try
        {
            // 第二次快取檢查，避免在等待鎖的過程中已經有其他執行緒重建了快取
            cached = await _cacheService.GetAsync<CacheEntry<DoctorDto>>(cacheKey);
            if (cached is not null)
                return cached.Value ?? throw new KeyNotFoundException("醫生不存在");

            // 確定快取沒有命中，從資料庫讀取資料
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);

            // 醫生不存在：寫入空值快取，防止快取穿透（短暫 TTL）
            if (doctor is null)
            {
                await _cacheService.SetAsync(cacheKey, new CacheEntry<DoctorDto>(null), CacheNullExpriation);
                throw new KeyNotFoundException("醫生不存在");
            }

            var dto = DoctorMapper.ToDto(doctor);
            await _cacheService.SetAsync(cacheKey, new CacheEntry<DoctorDto>(dto), CacheExpiration);
            return dto;
        }
        finally
        {
            // 釋放鎖，讓其他等待的執行緒可以繼續執行
            doctorSemaphore.Release();
        }
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
        _doctorSemaphores.TryRemove(doctorId, out _);
    }
}

// internal 修飾詞將此型別限制在此組件內，供測試專案透過 InternalsVisibleTo 存取
internal sealed record CacheEntry<T>(T? Value);
