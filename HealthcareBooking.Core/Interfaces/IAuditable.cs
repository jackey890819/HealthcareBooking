using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.Interfaces;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
