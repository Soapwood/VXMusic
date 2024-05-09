using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace VXMusic;

public class XSOverlay : INotificationClient
{
    public static bool IsReady;
    public static bool IsInitialising;

    private static string DefaultVxLogo;
    
    public XSOverlay()
    {
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
    public void SendNotification(string title, string content, int timeout)
    {
        SendNotificationInternal(title, content, timeout, DefaultVxLogo);
    }
    
    public void SendNotification(string title, string content, int timeout, string image)
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
            msg.messageType = 1;
            msg.title = title;
            msg.content = content;
            msg.height = 110f;
            msg.sourceApp = "VXMusic";
            msg.timeout = timeout;
            msg.audioPath = string.Empty;
            msg.useBase64Icon = true;
            msg.icon = image;

            var byteBuffer = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(msg);
            broadcastSocket.SendTo(byteBuffer, endPoint);
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