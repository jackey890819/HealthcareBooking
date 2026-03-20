using System;
using System.Collections.Generic;
using System.Text;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Repositories;

namespace HealthcareBooking.Core.Services;

public class BookService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClinicRepository _clinicRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookService(IAppointmentRepository appointmentRepository, IClinicRepository clinicRepository, IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _clinicRepository = clinicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> BookAppointmentAsync(int patientId, int clinicId)
    {
        // 嘗試獲取門診資訊。不存在則拋出異常
        var clinic = await _clinicRepository.GetByIdAsync(clinicId) ?? 
            throw new KeyNotFoundException("門診不存在");
        // 檢查門診是否已滿額。滿額則拋出異常
        if (clinic.CurrentBooked >= clinic.MaxQuota)
        {
            throw new InvalidOperationException("門診已滿");
        }
        // 更新門診已預約人數
        clinic.AddBooking();
        await _clinicRepository.UpdateAsync(clinic);
        // 創建預約
        var appointment = new Appointment
        {
            PatientId = patientId,
            ClinicId = clinicId,
            AppointmentDate = DateTime.Now // 暫時使用當前時間，實際應根據需求調整
        };
        await _appointmentRepository.AddAsync(appointment);
        await _unitOfWork.SaveChangesAsync();
        return appointment.Id;
    }
}
