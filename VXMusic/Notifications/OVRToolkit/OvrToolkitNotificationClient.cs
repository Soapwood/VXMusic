using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VXMusic.Connections.Websocket;
using VXMusic.OVRToolkit;

namespace VXMusic.OVRToolkit;

public class OvrToolkitNotificationClient : INotificationClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OvrToolkitNotificationClient> _logger;

    private static readonly Uri OvrToolkitServerUrl = new Uri("ws://127.0.0.1:11450/api");

    private WebSocketInteractor _webSocket;
    
    public static bool IsReady;
    public static bool IsInitialising;

    private static string DefaultVxLogo;
    
    public OvrToolkitNotificationClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<OvrToolkitNotificationClient>))
                      as ILogger<OvrToolkitNotificationClient> ??
                  throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogDebug("Creating OvrToolkitNotificationClient.");

        _webSocket = new WebSocketInteractor();

        IsInitialising = true;
        IsReady = IsOvrToolkitNotificationEndpointReady();
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
        // OVRToolkit doesn't like when either the title or content is empty
        // Need to sanitise this
        if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(title))
        {
            content = title;
            title = "VXMusic";
        }

        try
        {
            _webSocket.ConnectAsync(OvrToolkitServerUrl);
        }
        catch (Exception e)
        {
            Console.WriteLine($"This broke: {e}");
        }

        OVRToolkitNotificationBody jsonBody = new OVRToolkitNotificationBody()
        {
            title = title,
            body = content,
            icon = null
        };
        
        OVRToolkitNotification notification = new OVRToolkitNotification()
        {
            messageType = "SendNotification",
            json = JsonConvert.SerializeObject(jsonBody)
        }; 
        
        var contentsToSend = JsonConvert.SerializeObject(notification);
        _webSocket.SendAsync(contentsToSend);
    }

    private bool IsOvrToolkitNotificationEndpointReady()
    {
        try
        {
            _webSocket.ConnectAsync(OvrToolkitServerUrl);
            return true;
        }
        catch (SocketException se)
        {
            return false;
        }
    }
    
    public async Task ConnectAsync(Uri serverUri)
    {
        try
        {
            Console.WriteLine("Connecting to OVRToolkit Websocket Server...");
            await _webSocket.ConnectAsync(OvrToolkitServerUrl);
            Console.WriteLine("Connected OVRToolkit Websocket Server to server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server: {ex.Message}");
        }
    }
}