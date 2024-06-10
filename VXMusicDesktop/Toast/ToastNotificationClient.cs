using System;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using VXMusic;

namespace VXMusicDesktop.Toast;

public class ToastNotificationClient : INotificationClient
{
    private readonly Window _window;
    public bool ShouldToastNotificationsBeShown;
    
    private readonly Notifier _longNotifier;
    private readonly Notifier _shortNotifier;
    
    private const int LongNotifierTimeout = 5;
    private const int ShortNotifierTimeout = 3;

    public ToastNotificationClient(Window window)
    {
        _window = window;
        _longNotifier = CreateNotifier(window, TimeSpan.FromSeconds(LongNotifierTimeout));
        _shortNotifier = CreateNotifier(window, TimeSpan.FromSeconds(ShortNotifierTimeout));

        ShouldToastNotificationsBeShown = true;
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
        if(ShouldToastNotificationsBeShown)
            GetToastCall(level, title, content, timeout);
    }

    public void SendNotification(NotificationLevel level, string title, string content, int timeout)
    {
        if(ShouldToastNotificationsBeShown)
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