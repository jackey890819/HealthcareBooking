using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.Entities;

/// <summary>
/// 門診
/// </summary>
public class Clinic
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DateTime ClinicDate { get; set; }
    public int MaxQuota { get; set; }
    public int CurrentBooked { get; set; }
    // 用來實現樂觀鎖(Optimistic Concurrency Control)，確保在更新診所資訊時不會發生衝突
    public byte[] RowVersion { get; set; } = [];

    // 導航屬性，表示門診與醫生之間的關聯
    public required Doctor Doctor { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = [];
}
