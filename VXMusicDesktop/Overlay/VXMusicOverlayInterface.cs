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

namespace VXMusic.Overlay;

public class VXMusicOverlayInterface
{
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();
    private static bool _isProcessing;

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
                Logger.LogInformation("Listening for Request from VXMusic Overlay");
                await serverStream.WaitForConnectionAsync();
                _isProcessing = true;
                
                using (StreamReader reader = new StreamReader(serverStream))
                using (StreamWriter writer = new StreamWriter(serverStream) { AutoFlush = true })
                {
                    string eventData = await reader.ReadLineAsync();
                    Logger.LogDebug($"Received event from Unity Client: {eventData}");
                    await ProcessIncomingUnityEventMessage(writer, eventData);
                }
            }
        }
    }

    public static async Task<bool> ProcessIncomingUnityEventMessage(StreamWriter writer, string incomingMessage)
    {
        switch (incomingMessage)
        {
            case "VX_RECOGNITION_REQ": 
                Logger.LogDebug($"Sending event to Unity Client: {VXMMessages.RECOGNITION_ACKNOWLEDGE}");
                writer.WriteLine(VXMMessages.RECOGNITION_ACKNOWLEDGE);
                await RunRecognition();
                Logger.LogDebug($"Sending event to Unity Client: {VXMMessages.RECOGNITION_FINISH}");
                writer.WriteLine(VXMMessages.RECOGNITION_FINISH);
                return true;
            case "VX_CONNECT_REQ":
                // Send the connection ack response back to Unity
                writer.WriteLine(VXMMessages.CONNECTION_ACKNOWLEDGE);
                Logger.LogInformation($"Connected to Unity Overlay Client!");
                return true;
            default:
                Logger.LogError($"UNRECOGNISED MESSAGE SENT FROM UNITY TO VXMUSIC: {incomingMessage}");
                return false;
        }
    }

    public static async Task RunRecognition()
    {
        // TODO Two recognitions can run at the same time, add check to disable button if it's already running
        Logger.LogInformation("Running Recognition Flow from Overlay Trigger.");
        await VXMusicActions.PerformRecognitionFlow();
    }
}