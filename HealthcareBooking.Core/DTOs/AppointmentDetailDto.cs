using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.DTOs;

public record AppointmentDetailDto
(
    int AppointmentId,
    DateTime AppointmentDate,
    string ClinicDate,
    string DoctorName
);
