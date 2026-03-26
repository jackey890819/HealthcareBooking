using System;
using System.Collections.Generic;
using System.Text;
using HealthcareBooking.Core.Interfaces;

namespace HealthcareBooking.Infrastructure.Services;

public class FakeEmailService : INotificationService
{
    public Task SendBookingSuccessNotificationAsync(int patientId, int clinicId)
    {
        // 模擬一段等待時間，代表發送郵件的過程
        Task.Delay(500).Wait();

        // 在這裡我們只是簡單地輸出一條訊息，實際上應該是發送郵件給病患
        Console.WriteLine($"[FakeEmailService] 發送預約成功通知給病患 {patientId}，門診 {clinicId}");
        return Task.CompletedTask;
    }
}
