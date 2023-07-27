using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace VXMusic;

public class XSOverlay
{
    public static bool IsReady;
    public static bool IsInitialising;

    public XSOverlay()
    {
        IsInitialising = true;
        IsReady = IsXsOverlayNotificationEndpointReady();
        IsInitialising = false;
    }

    public void XSNotification(string Title, string Content, int Timeout, string Image = "")
    {
        if (IsReady || IsInitialising)
        {
            bool UseBase64Icon;
            string Icon;
            if (string.IsNullOrEmpty(Image))
            {
                UseBase64Icon = true;

                // Implies that the file is copied to the binary output folder
                Icon = File.ReadAllText(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Img", "VXLogo", "VXLogoSmallBase64.txt"));
            }
            else
            {
                UseBase64Icon = false;
                Icon = Image;
            }

            var broadcastIP = IPAddress.Loopback;
            var broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var endPoint = new IPEndPoint(broadcastIP, 42069);

            var msg = new XSOMessage();
            msg.messageType = 1;
            msg.title = Title;
            msg.content = Content;
            msg.height = 110f;
            msg.sourceApp = "VRCX";
            msg.timeout = Timeout;
            msg.audioPath = string.Empty;
            msg.useBase64Icon = UseBase64Icon;
            msg.icon = Icon;

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
}