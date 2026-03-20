namespace HealthcareBooking.Core.Entities;

/// <summary>
/// 預約
/// </summary>
public class Appointment
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }

    // 外來鍵 Foreign keys
    public int PatientId { get; set; }
    public int ClinicId { get; set; }

    // 導覽屬性 Navigation properties
    public Patient Patient { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
}
