namespace VXMusic;

public interface INotificationClient
{
    public void SendNotification(NotificationLevel level, string title, string content, int timeout, string image);
    public void SendNotification(NotificationLevel level, string title, string content, int timeout);
}