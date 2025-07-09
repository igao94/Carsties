using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Services;

public class NotificationsService(IHubContext<NotificationHub> hubContext) : INotificationsService
{
    public async Task NotifyAsync(string eventName, object message)
    {
        await hubContext.Clients.All.SendAsync(eventName, message);
    }
}
