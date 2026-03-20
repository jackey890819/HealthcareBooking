using System;
using System.Collections.Generic;
using System.Text;
using HealthcareBooking.Core.Interfaces;

namespace HealthcareBooking.Core.Entities;

public class Patient : IAuditable, ISoftDeletable
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // 導覽屬性 Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = [];

}
