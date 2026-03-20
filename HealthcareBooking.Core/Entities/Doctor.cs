using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.Entities;

public class Doctor
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

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
