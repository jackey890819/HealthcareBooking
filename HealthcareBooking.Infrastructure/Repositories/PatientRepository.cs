using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBooking.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;
    public PatientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Patient patient)
    {
        await _context.Patients.AddAsync(patient);
    }

    public async Task DeleteAsync(int patientId)
    {
        Patient? patient = await _context.Patients.FindAsync(patientId);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
        }
    }

    public async Task<IReadOnlyList<Patient>> GetAllAsync()
    {
        return await _context.Patients.ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(int patientId)
    {
        return await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
    }

    public Task UpdateAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 【進階查詢示範】一次查出病患的完整預約歷史，包含每次預約的門診資訊和醫生資訊
    /// </summary>
    /// <param name="patientId"></param>
    /// <returns></returns>
    public async Task<Patient?> GetPatientWithHistoryAsync(int patientId)
    {
        return await _context.Patients
            // 1. 唯讀最佳化：告訴 EF 不用追蹤這個物件，省下大量記憶體！
            .AsNoTracking()
            // 2. 貪婪載入：解決 N+1 查詢問題
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Clinic)
                    .ThenInclude(c => c.Doctor)
            // 3. 【進階效能調校】：避免笛卡爾積 (Cartesian Explosion) 導致記憶體爆炸
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == patientId);
    }
}
