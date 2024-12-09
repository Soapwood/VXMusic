using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VXMusic.Utils;

namespace VXMusic.OVRToolkit;

public class OvrToolkitNotificationClient : INotificationClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OvrToolkitNotificationClient> _logger;

    private static readonly Uri OvrToolkitServerUrl = new Uri("ws://127.0.0.1:11450/api");

    private readonly ClientWebSocket _webSocket;
    
    public static bool IsInitialised;

    private static byte[] DefaultVxLogoBytes;
    
    public OvrToolkitNotificationClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<OvrToolkitNotificationClient>))
                      as ILogger<OvrToolkitNotificationClient> ??
                  throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogDebug("Creating OvrToolkitNotificationClient.");

        _webSocket = new ClientWebSocket();

        IsInitialised = ConnectToOvrToolKitEndpoint();

        DefaultVxLogoBytes = BitmapUtils.ConvertPngToByteArray(Path.Combine( "Overlay", "Images", "VXLogo.png"));
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
        SendNotificationInternal(title, content, timeout, DefaultVxLogoBytes);
    }
    
    public void SendNotification(NotificationLevel level, string title, string content, int timeout, string image)
    {
        // Unless default, images are base64 encoded
        byte[] imageBytes = BitmapUtils.ConvertBase64ToBitmapByteArray(image);
        SendNotificationInternal(title, content, timeout, imageBytes);
    }
    
    private void SendNotificationInternal(string title, string content, int timeout, byte[] image)
    {
        if (!IsConnected())
            IsInitialised = ConnectToOvrToolKitEndpoint();
        
        // OVRToolkit doesn't like when either the title or content is empty
        // Need to sanitise this
        if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(title))
        {
            content = title;
            title = "VXMusic";
        }

        OVRToolkitNotificationBody jsonBody = new OVRToolkitNotificationBody()
        {
            title = title,
            body = content,
            icon = image
        };
        
        OVRToolkitNotification notification = new OVRToolkitNotification()
        {
            messageType = "SendNotification",
            json = JsonConvert.SerializeObject(jsonBody)
        }; 

        var contentsToSend = JsonConvert.SerializeObject(notification);
        SendAsync(contentsToSend);
    }

    private bool ConnectToOvrToolKitEndpoint()
    {
        try
        {
            _logger.LogDebug("Connecting to OVRToolkit Websocket Server...");
            ConnectAsync(OvrToolkitServerUrl);
            _logger.LogDebug("Connected OVRToolkit Websocket Server to server.");
            return true;
        }
        catch (SocketException se)
        {
            _logger.LogError("Error connecting to server: {ex.Message}");
            return false;
        }
    }
    
        public bool IsConnected()
    {
        return _webSocket.State == WebSocketState.Open;
    }
    
    public async Task ConnectAsync(Uri serverUri)
    {
        try
        {
            _logger.LogDebug("Connecting to server...");
            await _webSocket.ConnectAsync(serverUri, CancellationToken.None);
            _logger.LogDebug("Connected to server.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error connecting to server: {ex.Message}");
        }
    }

    public async Task SendAsync(string message)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(bytes);
            _logger.LogTrace("Sending message to websocket.");
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else
        {
            _logger.LogError("WebSocket is not open.");
        }
    }

    public async Task ReceiveAsync()
    {
        var buffer = new byte[1024];
        var segment = new ArraySegment<byte>(buffer);

        while (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(segment, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogTrace("Server requested close. Closing connection...");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogTrace("Message received: " + message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error receiving data: {ex.Message}");
            }
        }
    }

    public async Task CloseAsync()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
            Console.WriteLine("WebSocket connection closed.");
        }
    }
    
    
}