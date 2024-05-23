using VXMusic;

namespace VXMusicDesktop.Toast;

public class ToastNotificationDecorator
{
    private readonly INotificationClient _innerClient;
    private readonly ToastNotificationClient _toastService;

    public ToastNotificationDecorator(INotificationClient innerClient, ToastNotificationClient toastClient)
    {
        _innerClient = innerClient;
        _toastService = toastClient;
    }

    public void SendNotification(NotificationLevel level, string title, string content, int timeout, string image)
    {
        // Send toast notification
        _toastService.SendNotification(level, title, content, timeout, image);

        // Forward the call to the notification client
        _innerClient.SendNotification(level, title, content, timeout, image);
    }
    
    public void SendNotification(NotificationLevel level, string title, string content, int timeout)
    {
        // Send toast notification
        _toastService.SendNotification(level, title, content, timeout);

        // Forward the call to the notification client
        _innerClient.SendNotification(level, title, content, timeout);
    }
}