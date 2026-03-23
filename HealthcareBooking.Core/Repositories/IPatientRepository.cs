using System;
using System.Collections.Generic;
using System.Text;
using HealthcareBooking.Core.Entities;

namespace HealthcareBooking.Core.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetPatientWithHistoryAsync(int patientId);
}
