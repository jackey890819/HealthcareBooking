using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.Interfaces;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
