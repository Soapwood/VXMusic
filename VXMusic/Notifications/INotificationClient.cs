namespace VXMusic;

public interface INotificationClient
{
    public void SendNotification(string Title, string Content, int Timeout, string Image = "");
}