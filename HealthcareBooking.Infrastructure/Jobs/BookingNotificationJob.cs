using Hangfire;
using HealthcareBooking.Core.Interfaces;

namespace HealthcareBooking.Infrastructure.Jobs;

public class BookingNotificationJob(INotificationService notificationService)
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [300])]
    public Task ExecuteAsync(int patientId, int clinicId)
        => notificationService.SendBookingSuccessNotificationAsync(patientId, clinicId);
}
