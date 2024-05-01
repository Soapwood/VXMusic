using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
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

    private static bool _isProcessing;

    private static readonly string VxOverlayHeartBeatPipeName = "VXMusicOverlayEventPipe";
    private static readonly string VxOverlayServerPipeName = "VXMusicOverlayEventServerPipe";

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
            WindowStyle = ProcessWindowStyle.Hidden
            //WindowStyle = ProcessWindowStyle.Minimized
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

        StartVXMusicDesktopEventListener();
        
        // Need to send payload every time the overlay connects
        //SendOverlayInitPayload();

        return overlayProcess.Id;
    }

    public static async Task StartVXMusicDesktopEventListener()
    {
        Logger.LogInformation("Waiting for Unity Runtime Client to Connect...");
        
        while (true)
        {
            using (ServerStream =
                   new NamedPipeServerStream(VxOverlayHeartBeatPipeName, PipeDirection.InOut))
            {
                Logger.LogTrace("Listening for Request from VXMusic Overlay");
                await ServerStream.WaitForConnectionAsync();
                _isProcessing = true;
                
                using (ServerReader = new StreamReader(ServerStream))
                using (ServerWriter = new StreamWriter(ServerStream) { AutoFlush = true })
                {
                    string eventData = await ServerReader.ReadLineAsync();
                    Logger.LogTrace($"Received event from Unity Client: {eventData}");
                    await ProcessIncomingUnityEventMessage(ServerWriter, eventData);
                }
            }
        }
    }

    private static async Task<bool> SendOverlayInitPayload()
    {
        Logger.LogDebug($"Sending Overlay Init Payload to VXMusicOverlay: {App.VXMusicSession.OverlaySettings.ToPayload()}");
        return await SendMessageToVxMusicOverlay(App.VXMusicSession.OverlaySettings.ToPayload());
    }
    
    public static async Task<bool> SendOverlayAnchorUpdateRequest(string messageContent)
    {
        return await SendMessageToVxMusicOverlay(messageContent);
    }
    
    private static async Task<bool> SendMessageToVxMusicOverlay(string messageContent)
    {
        // Encapsulate resource management with 'using' to ensure proper disposal
        using (var clientStream = new NamedPipeClientStream(".", VxOverlayServerPipeName, PipeDirection.InOut))
        {
            try
            {
                // Connect to the server with a timeout (optional but recommended)
                clientStream.Connect(2000); // Timeout in milliseconds

                // Initialize StreamReader and StreamWriter using 'using' to automatically dispose
                using (var clientWriter = new StreamWriter(clientStream) { AutoFlush = true })
                using (var clientReader = new StreamReader(clientStream))
                {
                    // Send a request asynchronously
                    Logger.LogTrace($"Sending message to VXMusic Overlay: {messageContent}");
                    await clientWriter.WriteLineAsync(messageContent);

                    // Optionally read response back if expected
                    var response = await clientReader.ReadLineAsync();
                    Logger.LogTrace($"Received response from VXMusic Overlay: {response}");
                    Logger.LogDebug("Successfully sent request to VXMusic Overlay.");
                }
            }
            catch (TimeoutException ex)
            {
                Logger.LogError($"Failed to connect: {ex.Message}");
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                //Logger.LogError($"An error occurred: {ex.Message}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"An error occurred: {ex.Message}");
                return false;
            }
        }

        // Optionally handle further actions after successful communication
        // VXMusicSession.NotificationClient.SendNotification("VXMusic Overlay Connected!", "", 4);
        // SharedViewModel.IsOverlayRunning = true;
        // OverlayWasRunning = true;

        return true;
    }

    public static async Task<bool> ProcessIncomingUnityEventMessage(StreamWriter writer, string incomingMessage)
    {
        switch (incomingMessage)
        {
            case VXMMessage.CONNECTION_HEARTBEAT_REQUEST:
                // Send the connection ack response back to Unity
                writer.WriteLine(VXMMessage.CONNECTION_HEARTBEAT_ACKNOWLEDGE);
                SharedViewModel.IsOverlayRunning = true;
                HasNewHeartbeatMessage = true; // Flush has new HeartbeatMessage flag
                return true;
            case VXMMessage.RECOGNITION_REQUEST:
                return await HandleRecognitionRequest(writer);
            case VXMMessage.CONNECTION_REQUEST:
                return await HandleOverlayConnectionRequest(writer);
            case VXMMessage.CONNECTION_TERMINATION:
                // Send the connection ack response back to Unity
                Logger.LogInformation($"Received a Termination message from VXMusicOverlay!");
                SharedViewModel.IsOverlayRunning = false;
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

    public static async Task<bool> HandleRecognitionRequest(StreamWriter writer)
    {
        RecognitionApi currentRecognitionApi = App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi;
        
        if (!IsCurrentRecognitionClientConnected(currentRecognitionApi))
        {
            Logger.LogError($"Recognition Failed. {currentRecognitionApi} is not connected.");
            SharedViewModel.IsRecognitionRunning = false;
            VXMusicSession.NotificationClient.SendNotification("Recognition Failed", $"{currentRecognitionApi} is not connected! Check your Recognition settings.", 8);
            return false;
        }
        
        if (!SharedViewModel.IsRecognitionRunning)
        {
            Logger.LogDebug($"Sending event to VXMusicOverlay: {VXMMessage.RECOGNITION_ACKNOWLEDGE}");
            writer.WriteLine(VXMMessage.RECOGNITION_ACKNOWLEDGE);
            
            Logger.LogInformation("Running Recognition Flow from VXMusic Overlay.");
            SharedViewModel.IsRecognitionRunning = true;
            bool isFinished = await VXMusicActions.PerformRecognitionFlow();
            SharedViewModel.IsRecognitionRunning = false;
            
            Logger.LogDebug($"Sending event to VXMusicOverlay: {VXMMessage.RECOGNITION_FINISH}");
            writer.WriteLine(VXMMessage.RECOGNITION_FINISH);
            
            return true;
        }
        
        return false;
    }

    public static async Task<bool> HandleOverlayConnectionRequest(StreamWriter writer)
    {
        // Send the connection ack response back to Unity
        if (!SharedViewModel.IsOverlayRunning)
        {
            writer.WriteLine(VXMMessage.CONNECTION_ACKNOWLEDGE);
            Logger.LogInformation($"Connected to VXMusicOverlay!");
            VXMusicSession.NotificationClient.SendNotification("VXMusic Overlay Connected!", "",4);
            SharedViewModel.IsOverlayRunning = true;
            OverlayWasRunning = true;

            SendOverlayInitPayload();
            
            return true;
        }

        Logger.LogWarning("Overlay tried to connect but it is already connected.");
        return false;
    }
}