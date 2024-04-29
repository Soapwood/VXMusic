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
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();
    private static bool _isProcessing;

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

        StartVXMusicServerStream();
        
        return overlayProcess.Id;
    }

    public static async Task StartVXMusicServerStream()
    {
        Logger.LogInformation("Waiting for Unity Runtime Client to Connect...");
        
        while (true)
        {
            using (NamedPipeServerStream serverStream =
                   new NamedPipeServerStream("VXMusicOverlayEventPipe", PipeDirection.InOut))
            {
                Logger.LogTrace("Listening for Request from VXMusic Overlay");
                await serverStream.WaitForConnectionAsync();
                _isProcessing = true;
                
                using (StreamReader reader = new StreamReader(serverStream))
                using (StreamWriter writer = new StreamWriter(serverStream) { AutoFlush = true })
                {
                    string eventData = await reader.ReadLineAsync();
                    Logger.LogTrace($"Received event from Unity Client: {eventData}");
                    await ProcessIncomingUnityEventMessage(writer, eventData);
                }
            }
        }
    }
    
    public static async Task<bool> SendOverlayAnchorUpdateRequest(string overlayRequest)
    {
        // Create and connect the ClientStream
        var clientStream = new NamedPipeClientStream(".", "VXMusicOverlayEventPipeHandUpdate", PipeDirection.InOut);
        clientStream.Connect();
    
        // Initialize ClientReader and ClientWriter
        var clientReader = new StreamReader(clientStream);
        var clientWriter = new StreamWriter(clientStream) { AutoFlush = true };
        
        clientWriter.WriteLine(overlayRequest);
        
        // using (NamedPipeServerStream serverStream =
        //        new NamedPipeServerStream("VXMusicOverlayEventPipeHandUpdate", PipeDirection.InOut))
        // {
        //     Logger.LogDebug("Sending Overlay Anchor change request to VXMusicOverlay");
        //     await serverStream.WaitForConnectionAsync();
        //     //_isProcessing = true;
        //         
        //     using (StreamReader reader = new StreamReader(serverStream))
        //     using (StreamWriter writer = new StreamWriter(serverStream) { AutoFlush = true })
        //     {
        //         writer.WriteLine(overlayRequest);
        //
        //         //string eventData = await reader.ReadLineAsync();
        //         //Logger.LogTrace($"Received event from Unity Client: {eventData}");
        //         //await ProcessIncomingUnityEventMessage(writer, eventData);
        //     }
        // }
        
        
        //VXMusicSession.NotificationClient.SendNotification("VXMusic Overlay Connected!", "",4);
        //SharedViewModel.IsOverlayRunning = true;
        //OverlayWasRunning = true;
        return true;


        //Logger.LogWarning("Overlay tried to connect but it is already connected.");
        //return false;
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
            return true;
        }

        Logger.LogWarning("Overlay tried to connect but it is already connected.");
        return false;
    }
}