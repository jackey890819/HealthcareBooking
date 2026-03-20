using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.Entities;

/// <summary>
/// 門診
/// </summary>
public class Clinic
{
    public int Id { get; private set; }
    public int DoctorId { get; private set; }
    public DateTime ClinicDate { get; private set; }
    public int MaxQuota { get; private set; }
    public int CurrentBooked { get; private set; } = 0;
    // 用來實現樂觀鎖(Optimistic Concurrency Control)，確保在更新診所資訊時不會發生衝突
    public byte[] RowVersion { get; private set; } = [];

    // 導航屬性，表示門診與醫生之間的關聯
    public Doctor Doctor { get; private set; } = null!;
    public ICollection<Appointment> Appointments { get; private set; } = [];

    private Clinic() { } // EF Core 需要一個無參數的建構子來創建實體
    public Clinic(int doctorId, DateTime clinicDate, int maxQuota)
    {
        UpdateDetails(doctorId, clinicDate, maxQuota);
        //CurrentBooked = 0; // 預設已預約人數為0
    }

    public void UpdateDetails(int doctorId, DateTime clinicDate, int maxQuota)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(doctorId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxQuota);

        if (maxQuota < CurrentBooked)
        {
            throw new InvalidOperationException("最大名額不可小於目前已預約人數");
        }

        DoctorId = doctorId;
        ClinicDate = clinicDate;
        MaxQuota = maxQuota;
    }

    public void AddBooking()
    {
        if (CurrentBooked >= MaxQuota)
        {
            throw new InvalidOperationException("門診已滿");
        }
        CurrentBooked++;
    }
}
