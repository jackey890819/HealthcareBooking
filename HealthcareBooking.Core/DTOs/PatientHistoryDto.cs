using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.DTOs;

public record PatientHistoryDto
(
    int PatientId,
    string PatientName,
    List<AppointmentDetailDto> Appointments
);
