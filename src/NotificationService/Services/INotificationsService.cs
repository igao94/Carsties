namespace NotificationService.Services;

public interface INotificationsService
{
    Task NotifyAsync<T>(string eventName, T message);
}
