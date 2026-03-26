using System;
using System.Collections.Generic;
using System.Text;

namespace HealthcareBooking.Core.Interfaces;

public interface INotificationService
{
    // 同步發送
    Task SendBookingSuccessNotificationAsync(int patientId, int clinicId);
}
