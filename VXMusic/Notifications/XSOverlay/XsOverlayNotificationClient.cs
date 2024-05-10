using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using UltraLiteDB;

namespace VXMusic;

public class XsOverlayNotificationClient : INotificationClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<XsOverlayNotificationClient> _logger;
    
    public static bool IsReady;
    public static bool IsInitialising;

    private static string DefaultVxLogo;
    
    public XsOverlayNotificationClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<XsOverlayNotificationClient>))
                      as ILogger<XsOverlayNotificationClient> ??
                  throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogDebug("Creating XsOverlayNotificationClient.");
        
        IsInitialising = true;
        IsReady = IsXsOverlayNotificationEndpointReady();
        IsInitialising = false;
        
        DefaultVxLogo = File.ReadAllText(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Img", "VXLogo", "VXLogoSmallBase64.txt"));
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
            var broadcastIP = IPAddress.Loopback;
            var broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var endPoint = new IPEndPoint(broadcastIP, 42069);

            var msg = new XSOMessage();
            msg.messageType = 1; // TODO What are the types here?
            msg.title = title;
            msg.content = content;
            msg.height = 110f;
            msg.sourceApp = "VXMusic";
            msg.timeout = timeout;
            msg.audioPath = string.Empty;
            msg.useBase64Icon = true;
            msg.icon = image;

            var byteBuffer = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            try
            {
                broadcastSocket.SendTo(byteBuffer, endPoint);
            }
            catch (SocketException se)
            {
                // If the cover art Base64 encoding is too large it can be refused by XSOverlay
                // Retry with default logo.
                _logger.LogWarning("Album Art received from Recognition API was too large for sending to XSOverlay. Trying again with default VXMusic Logo.");
                msg.icon = DefaultVxLogo;
                byteBuffer = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
                broadcastSocket.SendTo(byteBuffer, endPoint);
            }
            
            broadcastSocket.Close();
        }
    }

    private bool IsXsOverlayNotificationEndpointReady()
    {
        try
        {
            using (var client = new UdpClient(IPAddress.Loopback.ToString(), 42069))
                return true;
        }
        catch (SocketException se)
        {
            return false;
        }
    }

    private struct XSOMessage
    {
        public int messageType { get; set; }
        public int index { get; set; }
        public float volume { get; set; }
        public string audioPath { get; set; }
        public float timeout { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string icon { get; set; }
        public float height { get; set; }
        public float opacity { get; set; }
        public bool useBase64Icon { get; set; }
        public string sourceApp { get; set; }
    }

    public class XSOAlert
    {
        public static string Default = "default";
        public static string Warning = "warning";
        public static string Error = "Error";
    }
}