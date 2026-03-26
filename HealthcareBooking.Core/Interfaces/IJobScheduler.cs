namespace HealthcareBooking.Core.Interfaces;

public interface IJobScheduler
{
    void EnqueueBookingNotification(int patientId, int clinicId);
}
