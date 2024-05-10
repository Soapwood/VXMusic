using System.Windows;
using System.Xml;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;

namespace VXMusic.Windows;

public class ToastNotificationClient : INotificationClient
{
    private readonly Notifier _longNotifier;
    private readonly Notifier _shortNotifier;
    
    private const int LongNotifierTimeout = 5;
    private const int ShortNotifierTimeout = 3;

    public ToastNotificationClient(Window window)
    {
        _longNotifier = CreateNotifier(window, TimeSpan.FromSeconds(LongNotifierTimeout));
        _shortNotifier = CreateNotifier(window, TimeSpan.FromSeconds(ShortNotifierTimeout));
    }
    
    //TODO MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
    // https://github.com/rafallopatka/ToastNotifications/blob/master-v2/Docs/Configuration.md
    private Notifier CreateNotifier(Window window, TimeSpan lifeTime)
    {
        return new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: window,
                corner: Corner.BottomRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: lifeTime,
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));
        });
    }

    public void SendNotification(NotificationLevel level, string title, string content, int timeout, string image)
    {
        GetToastCall(level, title, content, timeout);
    }

    public void SendNotification(NotificationLevel level, string title, string content, int timeout)
    {
        GetToastCall(level, title, content, timeout);
    }

    private void GetToastCall(NotificationLevel level, string title, string content, int timeout)
    {
        Notifier toastNotifier = timeout <= ShortNotifierTimeout ? _shortNotifier : _longNotifier;
        
        switch (level)
        {
            case NotificationLevel.Warning:
                toastNotifier.ShowWarning($"{title} {content}");
                break;
            case NotificationLevel.Error:
                toastNotifier.ShowError($"{title} {content}");
                break;
            case NotificationLevel.Success:
                toastNotifier.ShowSuccess($"{title} {content}");
                break;
            case NotificationLevel.Info:
            default:
                toastNotifier.ShowInformation($"{title} {content}");
                break;
        }
    }

    public void Dispose()
    {
        _shortNotifier.Dispose();
        _longNotifier.Dispose();
    }
}