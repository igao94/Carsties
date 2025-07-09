namespace NotificationService.Services;

public interface INotificationsService
{
    Task NotifyAsync(string eventName, object message);
}
