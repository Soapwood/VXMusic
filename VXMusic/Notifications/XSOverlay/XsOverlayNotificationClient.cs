using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using XSSocket;
using XSSocket.Models;

namespace VXMusic;

public class XsOverlayNotificationClient : INotificationClient, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<XsOverlayNotificationClient> _logger;
    
    public static bool IsReady;
    public static bool IsInitialising;

    private static string DefaultVxLogo;
    private static int WebsocketPort = 42070;

    XSSocket.XSSocket XSOConnector;
    
    public XsOverlayNotificationClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<XsOverlayNotificationClient>))
                      as ILogger<XsOverlayNotificationClient> ??
                  throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogDebug("Creating XsOverlayNotificationClient.");
        
        IsInitialising = true;
        
        XSOConnector = new XSSocket.XSSocket("VXMusic");
        XSOConnector.ConnectAsync().ConfigureAwait(false);
        
        IsReady = IsXsOverlayNotificationEndpointReady();
        
        IsInitialising = false;
        
        DefaultVxLogo = File.ReadAllText(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay", "Images", "VXLogoSmallBase64.txt"));
    }

    /// <summary>
    /// Send Notification using Default VX Icon
    /// Use overload Method with Image paramater to use Image URL
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <param name="timeout"></param>
    public void SendNotification(NotificationLevel level, string title, string content, int timeout)
    {
       SendNotificationInternal(title, content, timeout, DefaultVxLogo);
    }
    
    public void SendNotification(NotificationLevel level, string title, string content, int timeout, string image)
    {
        SendNotificationInternal(title, content, timeout, image);
    }
    
    private void SendNotificationInternal(string title, string content, int timeout, string image = "")
    {
        if (IsReady || IsInitialising)
        {
            XSNotificationObject notificationObject =
                new XSNotificationObject()
                {
                    type = 1,
                    title = title,
                    timeout = timeout,
                    height = 80f,
                    content = content,
                    sourceApp = "VXMusic",
                    useBase64Icon = true,
                    icon = image
                };
        
            XSOConnector.SendNotification(notificationObject);
        }
    }

    private bool IsXsOverlayNotificationEndpointReady()
    {
        _logger.LogInformation("Waiting to connect to XSOverlay...");
        while (XSOConnector.State == WebSocketState.Connecting)
        {
            Task.Delay(500);
        }
        
        return XSOConnector.State == WebSocketState.Open;
    }
    
    public void Dispose()
    {
        XSOConnector.Disconnect();
        XSOConnector.Dispose();
    }
}