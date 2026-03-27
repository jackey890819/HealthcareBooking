using Microsoft.AspNetCore.SignalR;

namespace HealthcareBooking.API.Hubs;

public class ClinicHub : Hub
{
    public async Task JoinClinicGroup(int clinicId)
    {
        var groupName = $"Clinic_{clinicId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        // (選擇性) 你也可以在這裡回傳一條歡迎訊息給前端測試用
        // await Clients.Caller.SendAsync("ReceiveMessage", $"已成功訂閱門診 {clinicId} 的即時叫號");
        Console.WriteLine($"Connection {Context.ConnectionId} joined group {groupName}");
    }
}
