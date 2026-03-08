using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.Entities;

public class Doctor
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // 導覽屬性 Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = [];
}
