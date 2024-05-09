namespace VXMusic;

public interface INotificationClient
{
    public void SendNotification(string title, string content, int timeout, string image);
    public void SendNotification(string title, string content, int timeout);
}