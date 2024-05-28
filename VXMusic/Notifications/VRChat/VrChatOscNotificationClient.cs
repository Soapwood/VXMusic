using Microsoft.Extensions.Logging;
using VRChatOSCLib;

namespace VXMusic.VRChat;

public class VrChatOscNotificationClient : INotificationClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VrChatOscNotificationClient> _logger;

    private bool _isConnectedToVrChatRuntime;
    public static bool IsConnectedToVrChatRuntime;

    private VRChatOSC Instance;

    public VrChatOscNotificationClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<VrChatOscNotificationClient>))
                      as ILogger<VrChatOscNotificationClient> ??
                  throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogDebug("Creating VrChatOscNotificationClient.");
    }

    public void Connect()
    {
        Instance = new VRChatOSC(true);
        Instance.Connect(); // Uses default remote port 9000
        IsConnectedToVrChatRuntime = true;
    }
    
    public void Disconnect()
    {
        try
        {
            if (Instance != null)
            {
                Instance.Dispose();
                IsConnectedToVrChatRuntime = false;
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning("Tried to dispose of OSC Client instance but it was not created.");
        }
    }
    
    public void SendNotification(NotificationLevel level, string title, string content, int timeout, string image)
    {
        if(IsConnectedToVrChatRuntime)
            Instance.SendChatbox(title + content, true); // Bypass keyboard and sends string to Chatbox
    }

    public void SendNotification(NotificationLevel level, string title, string content, int timeout)
    {
        // TODO Use Timeout to start and end ChatBox message?
        if (IsConnectedToVrChatRuntime)
        {
            Instance.SendChatbox(title + content, true); // Bypass keyboard and sends string to Chatbox
        }
    }
    
    public async Task SendNotificationAsync(NotificationLevel level, string title, string content, int timeout)
    {
        // TODO Use Timeout to start and end ChatBox message?
        if (IsConnectedToVrChatRuntime)
        {
            Instance.SendChatbox(title + content, true); // Bypass keyboard and sends string to Chatbox
            await Task.Delay(timeout);
            Instance.SendChatbox("", true); // Bypass keyboard and sends string to Chatbox
        }
    }

    public void SendChatboxTypingIndicator(bool isTyping)
    {
        if(IsConnectedToVrChatRuntime)
            Instance.SendChatbox(isTyping);
    }
}