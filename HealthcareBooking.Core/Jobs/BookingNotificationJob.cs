using Hangfire;
using HealthcareBooking.Core.Interfaces;

namespace HealthcareBooking.Core.Jobs;

public class BookingNotificationJob(INotificationService notificationService)
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [300])]
    public Task ExecuteAsync(int patientId, int clinicId)
        => notificationService.SendBookingSuccessNotificationAsync(patientId, clinicId);
}
