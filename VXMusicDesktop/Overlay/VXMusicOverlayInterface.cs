using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic.Recognition.AudD;
using VXMusicDesktop;
using VXMusicDesktop.MVVM.ViewModel;

namespace VXMusic.Overlay;

public class VXMusicOverlayInterface
{
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<VXMusicOverlayInterface>>();
    
    public static NamedPipeServerStream ServerStream;
    public static StreamReader ServerReader;
    public static StreamWriter ServerWriter;

    private static TcpListener _listener;
    
    private static int _vxMusicDesktopTcpPort = 61820;
    private static int _vxMusicOverlayTcpPort = 61821;
    private static string _localhost = "127.0.0.1";

    private static bool _isProcessing;

    private static readonly string VxOverlayHeartBeatPipeName = "VXMusicOverlayEventPipe";

    public static bool HasNewHeartbeatMessage { get; set; }
    public static bool OverlayWasRunning { get; set; }

    public static SharedViewModel SharedViewModel { get; set; }

    public static int LaunchVXMOverlayRuntime(string runtimePath)
    {
        ProcessStartInfo overlayProcessStartInfo = new ProcessStartInfo
        {
            FileName = runtimePath,
            Arguments = "",
            UseShellExecute = false, // Set this to false to redirect output if needed
            CreateNoWindow = true, // Set this to true to hide the Unity window
            Verb = "runas", // TODO does this need to be here?
            WindowStyle = ProcessWindowStyle.Minimized
        };

        Process overlayProcess = new Process
        {
            StartInfo = overlayProcessStartInfo
        };

        overlayProcess.EnableRaisingEvents = true;
        overlayProcess.Exited += (sender, e) => { Logger.LogWarning("Closing down VXMusicOverlay."); };

        Logger.LogInformation("Starting VXMusic Overlay Runtime...");
        Logger.LogDebug($"Running {overlayProcessStartInfo.FileName} with args: [{overlayProcessStartInfo.Arguments}]");
        
        overlayProcess.Start();

        Logger.LogDebug($"VXMusicOverlay process running with PID: {overlayProcess.Id}");

        StartVXMusicDesktopHeartbeatListener();
        StartVXMusicDesktopTcpServer();
        
        //App.ToastNotification.send("Overlay Launched Successfully");

        // Need to send payload every time the overlay connects
        return overlayProcess.Id;
    }

    public static async Task StartVXMusicDesktopHeartbeatListener()
    {
        Logger.LogInformation("Waiting for Unity Runtime Client to Connect...");
        
        while (true)
        {
            using (ServerStream =
                   new NamedPipeServerStream(VXMMessage.VxMusicHeartbeatPipeName, PipeDirection.InOut))
            {
                Logger.LogTrace("Listening for Heartbeats from VXMusic Overlay");
                await ServerStream.WaitForConnectionAsync();
                _isProcessing = true;
                
                using (ServerReader = new StreamReader(ServerStream))
                using (ServerWriter = new StreamWriter(ServerStream) { AutoFlush = true })
                {
                    string eventData = await ServerReader.ReadLineAsync();
                    Logger.LogTrace($"Received Heartbeat from Unity Client: {eventData}");
                    await ProcessIncomingUnityHeartbeat(ServerWriter, eventData);
                }
            }
        }
    }

    public static async Task StartVXMusicDesktopTcpServer()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, _vxMusicDesktopTcpPort);
            _listener.Start();
        }
        catch (Exception ex)
        {
            Logger.LogError($"TCP Server failed to start! Desktop Port: {_vxMusicDesktopTcpPort} Overlay Port: {_vxMusicOverlayTcpPort}");
            Logger.LogError(ex.Message);
        }
        
        Logger.LogTrace($"VXMusic Desktop TCP Server started.");

        try
        {
            while (true)
            {
                Logger.LogTrace("Waiting for Overlay Messages...");
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Logger.LogDebug("Overlay connected for Message.");
                ProcessClientAsync(client);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Server error: {ex.Message}");
        }
        finally
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }
        }
        
    }
    
    private static async Task ProcessClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                await ListenForMessages(stream);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error processing client: {ex.Message}");
        }
    }
    
    private static async Task ListenForMessages(NetworkStream stream)
    {
        try
        {
            while (true)
            {
                ReadMessage(stream, out var message);
                if (message == null)
                {
                    Logger.LogWarning("Client disconnected or stream ended.");
                    break;
                }
                Logger.LogTrace("Received from VXMusic Overlay: " + message);
                ProcessIncomingUnityEventMessage(message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error in receiving data: " + ex.Message);
            // Handle the error (e.g., reconnect logic)
        }
    }

    private static Task<bool> SendOverlayInitPayload()
    {
        Logger.LogDebug($"Sending Overlay Init Payload to VXMusicOverlay: {App.VXMusicSession.OverlaySettings.ToPayload()}");
        return SendMessageToVxMusicOverlayOverTcp(App.VXMusicSession.OverlaySettings.ToPayload()); //await SendMessageToVxMusicOverlay(App.VXMusicSession.OverlaySettings.ToPayload());
    }
    
    public static Task<bool> SendOverlayAnchorUpdateRequest(string messageContent)
    {
        return SendMessageToVxMusicOverlayOverTcp(messageContent);
    }

    public static async Task<bool> SendMessageToVxMusicOverlayOverTcp(string messageContent)
    {
        using (TcpClient client = new TcpClient(_localhost, _vxMusicOverlayTcpPort))
        using (NetworkStream stream = client.GetStream())
        {
            await SendMessageAsync(stream, messageContent);

            // Wait for and read the acknowledgment from the server
            string response = await ReadMessageAsync(stream);
            Logger.LogTrace("Received from VXMusic Overlay: " + response);
        }
        return true;
    }
    
    private static async Task SendMessageAsync(NetworkStream stream, string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
        await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
        await stream.WriteAsync(data, 0, data.Length);
    }
    
    private static bool ReadMessage(NetworkStream stream, out string message)
    {
        message = null;
        byte[] lengthBytes = new byte[4];
        int totalBytesRead = 0;

        // Ensure all bytes for length are read
        while (totalBytesRead < 4)
        {
            int bytesRead = stream.Read(lengthBytes, totalBytesRead, 4 - totalBytesRead);
            if (bytesRead == 0)
            {
                return false; // Connection has been closed by the client.
            }
            totalBytesRead += bytesRead;
        }

        int length = BitConverter.ToInt32(lengthBytes, 0);
        byte[] buffer = new byte[length];
        totalBytesRead = 0;

        // Ensure all bytes for the message are read
        while (totalBytesRead < length)
        {
            int bytesRead = stream.Read(buffer, totalBytesRead, length - totalBytesRead);
            if (bytesRead == 0)
            {
                return false; // Connection has been closed by the client.
            }
            totalBytesRead += bytesRead;
        }

        message = Encoding.ASCII.GetString(buffer);
        return true;
    }
    
    private static async Task<string> ReadMessageAsync(NetworkStream stream)
    {
        byte[] lengthBytes = new byte[4];
        int bytesRead = await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
        if (bytesRead != lengthBytes.Length)
        {
            throw new Exception("Failed to read the message length from the stream.");
        }

        int length = BitConverter.ToInt32(lengthBytes, 0);
        byte[] buffer = new byte[length];
        int totalRead = 0;
        while (totalRead < length)
        {
            bytesRead = await stream.ReadAsync(buffer, totalRead, length - totalRead);
            if (bytesRead == 0)
            {
                throw new Exception("Connection closed prematurely.");
            }
            totalRead += bytesRead;
        }

        return Encoding.ASCII.GetString(buffer);
    }

    public static async Task<bool> ProcessIncomingUnityHeartbeat(StreamWriter writer, string incomingMessage)
    {
        switch (incomingMessage)
        {
            case VXMMessage.CONNECTION_REQUEST:
                return await HandleOverlayConnectionRequest(writer);
            case VXMMessage.CONNECTION_HEARTBEAT_REQUEST:
                writer.WriteLine(VXMMessage.CONNECTION_HEARTBEAT_ACKNOWLEDGE);
                SharedViewModel.IsOverlayRunning = true;
                HasNewHeartbeatMessage = true; // Flush has new HeartbeatMessage flag
                return true;
            case VXMMessage.CONNECTION_TERMINATION:
                Logger.LogInformation($"Received a Termination message from VXMusicOverlay!");
                SharedViewModel.IsOverlayRunning = false;
                return true;
            default:
                Logger.LogError($"UNRECOGNISED MESSAGE SENT FROM OVERLAY TO VXMUSIC: {incomingMessage}");
                return false;
        }
    }

    public static async Task<bool> ProcessIncomingUnityEventMessage(string incomingMessage)
    {
        switch (incomingMessage)
        {
            case VXMMessage.RECOGNITION_REQUEST:
                return await HandleRecognitionRequest();
            case VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_ACKNOWLEDGE:
                VXUserSettings.Overlay.SetOverlayAnchor(VXMusicOverlayAnchor.LeftHand); // TODO Move this logic out somewhere else
                App.VXMusicSession.OverlaySettings.OverlayAnchor = VXMusicOverlayAnchor.LeftHand;
                return true;
            case VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_ACKNOWLEDGE:
                VXUserSettings.Overlay.SetOverlayAnchor(VXMusicOverlayAnchor.RightHand);
                App.VXMusicSession.OverlaySettings.OverlayAnchor = VXMusicOverlayAnchor.RightHand;
                return true;
            default:
                Logger.LogError($"UNRECOGNISED MESSAGE SENT FROM UNITY TO VXMUSIC: {incomingMessage}");
                return false;
        }

        HasNewHeartbeatMessage = false;
    }

    private static bool IsCurrentRecognitionClientConnected(RecognitionApi currentRecognitionApi)
    {
        switch (currentRecognitionApi)
        {
            case RecognitionApi.Shazam:
                return SharedViewModel.IsShazamApiConnected;
            case RecognitionApi.AudD:
                return SharedViewModel.IsAudDApiConnected;
            case RecognitionApi.Unknown:
                return false;
            default:
                return false;
        }
    }

    public static async Task<bool> HandleRecognitionRequest()
    {
        RecognitionApi currentRecognitionApi = App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi;
        
        if (!IsCurrentRecognitionClientConnected(currentRecognitionApi))
        {
            Logger.LogError($"Recognition Failed. {currentRecognitionApi} is not connected.");
            SharedViewModel.IsRecognitionRunning = false;
            //App.ToastNotification.Error($"{currentRecognitionApi} is not connected! Check your Recognition settings.");
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Error, "Recognition Failed", $"{currentRecognitionApi} is not connected! Check your Recognition settings.", 8);
            App.ToastNotification.SendNotification(NotificationLevel.Error, "Recognition Failed", $"{currentRecognitionApi} is not connected! Check your Recognition settings.", 8);
            return false;
        }
        
        if (!SharedViewModel.IsRecognitionRunning)
        {
            Logger.LogTrace($"Sending event to VXMusicOverlay: {VXMMessage.RECOGNITION_ACKNOWLEDGE}");
            SendMessageToVxMusicOverlayOverTcp(VXMMessage.RECOGNITION_ACKNOWLEDGE);
            
            Logger.LogInformation("Running Recognition Flow from VXMusic Overlay.");
            SharedViewModel.IsRecognitionRunning = true;
            bool isFinished = await VXMusicActions.PerformRecognitionFlow();
            SharedViewModel.IsRecognitionRunning = false;
            
            Logger.LogTrace($"Sending event to VXMusicOverlay: {VXMMessage.RECOGNITION_FINISH}");
            SendMessageToVxMusicOverlayOverTcp(VXMMessage.RECOGNITION_FINISH);
            
            return true;
        }
        
        return false;
    }

    public static async Task<bool> HandleOverlayConnectionRequest(StreamWriter writer)
    {
        // Send the connection ack response back to Unity
        if (!SharedViewModel.IsOverlayRunning)
        {
            //SendMessageToVxMusicOverlayOverTcp(VXMMessage.CONNECTION_ACKNOWLEDGE);
            writer.WriteLine(VXMMessage.CONNECTION_ACKNOWLEDGE);
            Logger.LogInformation($"Connected to VXMusicOverlay!");
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Success,"VXMusic Overlay Connected!", "",4);
            App.ToastNotification.SendNotification(NotificationLevel.Success,"VXMusic Overlay Connected!", "",4);
            SharedViewModel.IsOverlayRunning = true;
            OverlayWasRunning = true;

            SendOverlayInitPayload();
            
            return true;
        }

        Logger.LogWarning("Overlay tried to connect but it is already connected.");
        return false;
    }
}


/////////////////////////
///
    // private static async Task<bool> SendMessageToVxMusicOverlay(string messageContent)
    // {
    //     // Encapsulate resource management with 'using' to ensure proper disposal
    //     using (var clientStream = new NamedPipeClientStream(".", VXMMessage.VxMusicOverlaySettingsPipe, PipeDirection.InOut))
    //     {
    //         try
    //         {
    //             // Connect to the server with a timeout (optional but recommended)
    //             clientStream.Connect(5000); // Timeout in milliseconds
    //
    //             // Initialize StreamReader and StreamWriter using 'using' to automatically dispose
    //             using (var clientWriter = new StreamWriter(clientStream) { AutoFlush = true })
    //             using (var clientReader = new StreamReader(clientStream))
    //             {
    //                 // Send a request asynchronously
    //                 Logger.LogTrace($"Sending message to VXMusic Overlay: {messageContent}");
    //                 await clientWriter.WriteLineAsync(messageContent);
    //
    //                 // Optionally read response back if expected
    //                 var response = await clientReader.ReadLineAsync();
    //                 Logger.LogTrace($"Received response from VXMusic Overlay: {response}");
    //                 Logger.LogDebug("Successfully sent request to VXMusic Overlay.");
    //             }
    //         }
    //         catch (TimeoutException ex)
    //         {
    //             Logger.LogError($"Failed to connect: {ex.Message}");
    //             return false;
    //         }
    //         catch (ObjectDisposedException ex)
    //         {
    //             //Logger.LogError($"An error occurred: {ex.Message}");
    //             return true;
    //         }
    //         catch (Exception ex)
    //         {
    //             Logger.LogError($"An error occurred: {ex.Message}");
    //             return false;
    //         }
    //     }
    //
    //     // Optionally handle further actions after successful communication
    //     // VXMusicSession.NotificationClient.SendNotification("VXMusic Overlay Connected!", "", 4);
    //     // SharedViewModel.IsOverlayRunning = true;
    //     // OverlayWasRunning = true;
    //
    //     return true;
    // }