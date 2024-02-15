using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusicDesktop;

namespace VXMusic.Overlay;

public class VXMusicOverlayInterface
{
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();

    public static void LaunchVXMOverlayRuntime(string runtimePath)
    {
        ProcessStartInfo overlayProcessStartInfo = new ProcessStartInfo
        {
            FileName = runtimePath,
            Arguments = "",
            UseShellExecute = false, // Set this to false to redirect output if needed
            CreateNoWindow = true, // Set this to true to hide the Unity window
            //WindowStyle = ProcessWindowStyle.Minimized
        };

        Process unityProcess = new Process
        {
            StartInfo = overlayProcessStartInfo
        };

        unityProcess.EnableRaisingEvents = true;
        unityProcess.Exited += (sender, e) => { Console.WriteLine("Closing down VXMOverlay."); };

        Logger.LogInformation("Starting VXMusic Overlay Runtime...");
        Logger.LogDebug($"Running {overlayProcessStartInfo.FileName} with args: [{overlayProcessStartInfo.Arguments}]");
        unityProcess.Start();
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
            case "VX_TRIGGER_RECOGNITION":
                Logger.LogDebug($"Sending event to Unity Client: {VXMMessages.RECOGNITION_ACKNOWLEDGE}");
                writer.WriteLine(VXMMessages.RECOGNITION_ACKNOWLEDGE);
                await RunRecognition();
                Logger.LogDebug($"Sending event to Unity Client: {VXMMessages.RECOGNITION_FINISH}");
                writer.WriteLine(VXMMessages.RECOGNITION_FINISH);
                return true;
            case "VX_CONNECT_REQ":
                // Send the connection ack response back to Unity
                writer.WriteLine(VXMMessages.CONNECTION_ACKNOWLEDGE);
                return true;
            default:
                Logger.LogError($"UNRECOGNISED MESSAGE SENT FROM UNITY TO VXMUSIC: {incomingMessage}");
                return false;
        }
    }

    public static async Task<bool> RunRecognition()
    {
        // TODO Two recognitions can run at the same time, add check to disable button if it's already running

        await Task.Run(() =>
        {
            VXMusicSession.RecordingClient.StartRecording();

            VXMusicSession.NotificationClient.SendNotification("VXMusic is Listening...", "",
                VXMusicSession.RecordingClient.GetRecordingTimeSeconds());

            while (!VXMusicSession.RecordingClient.IsCaptureStateStopped())
            {
                Thread.Sleep(500);
            }

            VXMusicSession.RecordingClient.StopRecording();
        });

        VXMusicSession.NotificationClient.SendNotification("Sounds great! Just a moment..", "", 2);

        //var result = //await VXMusicAPI.RunRecognition();
        var result = await VXMusicSession.RecognitionClient.RunRecognition();

        if (result.Status == Status.Error)
        {
            VXMusicSession.NotificationClient.SendNotification("Recognition failed! Oh jaysus", "", 5);
            Console.WriteLine("Recognition failed! Oh jaysus");
            //Environment.Exit(0);
        }
        else if (result.Status == Status.NoMatches || result.Result == null)
        {
            VXMusicSession.NotificationClient.SendNotification("Oops, couldn't get that.",
                "Tech Tip: Have you tried turning up your World Volume?", 5);
            Console.WriteLine("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
        }
        else if (result.Status == Status.RecordingError)
        {
            VXMusicSession.NotificationClient.SendNotification("I couldn't hear anything!",
                "Something messed up when recording audio. Check your audio device.", 10);
            Console.WriteLine(
                "I couldn't hear anything! Something messed up when recording audio. Check your audio device.");
        }
        else
        {
            VXMusicSession.NotificationClient.SendNotification($"{result.Result.Artist} - {result.Result.Title}",
                $"{result.Result.Album} ({result.Result.ReleaseDate})", 8);
            Console.WriteLine(
                $"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");
        }

        return true;
    }
}