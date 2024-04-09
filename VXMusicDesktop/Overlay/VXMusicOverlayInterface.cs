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
        overlayProcess.Exited += (sender, e) => { Console.WriteLine("Closing down VXMOverlay."); };

        Logger.LogInformation("Starting VXMusic Overlay Runtime...");
        Logger.LogDebug($"Running {overlayProcessStartInfo.FileName} with args: [{overlayProcessStartInfo.Arguments}]");
        
        overlayProcess.Start();

        Logger.LogDebug($"VXMusicOverlay process running with PID: {overlayProcess.Id}");
        
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
                Logger.LogDebug($"Sending event to VXMusicOverlay: {VXMMessage.RECOGNITION_ACKNOWLEDGE}");
                writer.WriteLine(VXMMessage.RECOGNITION_ACKNOWLEDGE);
                await RunRecognition();
                Logger.LogDebug($"Sending event to VXMusicOverlay: {VXMMessage.RECOGNITION_FINISH}");
                writer.WriteLine(VXMMessage.RECOGNITION_FINISH);
                return true;
            case VXMMessage.CONNECTION_REQUEST:
                // Send the connection ack response back to Unity
                writer.WriteLine(VXMMessage.CONNECTION_ACKNOWLEDGE);
                Logger.LogInformation($"Connected to VXMusicOverlay!");
                SharedViewModel.IsOverlayRunning = true;
                return true;
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

    public static async Task RunRecognition()
    {
        if (!SharedViewModel.IsRecognitionRunning)
        {
            Logger.LogInformation("Running Recognition Flow from VXMusic Overlay.");
            SharedViewModel.IsRecognitionRunning = true;
            bool isFinished = await VXMusicActions.PerformRecognitionFlow();
            SharedViewModel.IsRecognitionRunning = false;
        }
    }
}