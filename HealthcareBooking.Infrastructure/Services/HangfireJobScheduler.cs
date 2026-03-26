using Hangfire;
using HealthcareBooking.Core.Interfaces;
using HealthcareBooking.Infrastructure.Jobs;

namespace HealthcareBooking.Infrastructure.Services;

public class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireJobScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueBookingNotification(int patientId, int clinicId)
    {
        // 這裡才真正呼叫 Hangfire，並指定執行我們在 Infrastructure 的 Job
        _backgroundJobClient.Enqueue<BookingNotificationJob>(job => job.ExecuteAsync(patientId, clinicId));
    }
}