using System;
using System.Collections.Generic;
using System.Text;
using HealthcareBooking.Core.Interfaces;

namespace HealthcareBooking.Core.Entities;

public class Doctor : IAuditable, ISoftDeletable
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // 導覽屬性 Navigation properties
    public ICollection<Clinic> Clinics { get; set; } = [];

    private Doctor() { }

    public Doctor(string name)
    {
        UpdateName(name);
    }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}
