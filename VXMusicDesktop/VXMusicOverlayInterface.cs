using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using VXMusicDesktop;

namespace VXMusic.Overlay;

public class VXMusicOverlayInterface
{
    public static NamedPipeServerStream PipeServer;

    public static void LaunchVXMOverlayRuntime(string runtimePath)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = runtimePath,
            Arguments = "",
            UseShellExecute = false, // Set this to false to redirect output if needed
            CreateNoWindow = false, // Set this to true to hide the Unity window
            //WindowStyle = ProcessWindowStyle.Minimized
        };

        Process unityProcess = new Process
        {
            StartInfo = startInfo
        };

        unityProcess.EnableRaisingEvents = true;
        unityProcess.Exited += (sender, e) => { Console.WriteLine("Closing down VXMOverlay."); };

        Console.WriteLine("Starting VXMusic Overlay");
        unityProcess.Start();
    }

    public static async Task StartVXMusicServerStream()
    {
        while (true)
        {
            using (NamedPipeServerStream serverStream =
                   new NamedPipeServerStream("VXMusicOverlayEventPipe", PipeDirection.InOut))
            {
                Console.WriteLine("Waiting for Unity client to connect...");
                await serverStream.WaitForConnectionAsync();

                using (StreamReader reader = new StreamReader(serverStream))
                using (StreamWriter writer = new StreamWriter(serverStream) { AutoFlush = true })
                {
                    //await serverStream.WaitForConnectionAsync();

                    
                    string eventData = await reader.ReadLineAsync();
                    //while ((eventData = await reader.ReadLineAsync()) != null)
                    //{
                        Console.WriteLine($"Received event from Unity: {eventData}");
                        await ProcessIncomingUnityEventMessage(writer, eventData);

                        //await writer.WriteLineAsync(line);
                    //}

                    // while (true)
                    // {
                    //     //reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    //     string eventData = reader.ReadLine();
                    //     if (eventData != null)
                    //     {
                    //         // Process the received event data
                    //
                    //         await ProcessIncomingUnityEventMessage(writer, eventData);
                    //         //return;
                    //     }
                    //     // Process the message and prepare a response
                    //     //string response = "Response from .NET: " + message.ToUpper();
                    //
                    //     // Send the response back to Unity
                    //     // writer.WriteLine(response);
                    //     // writer.Flush();
                    // }
                }
            }
        }
    }

    public static async Task<bool> ProcessIncomingUnityEventMessage(StreamWriter writer, string incomingMessage)
    {
        switch (incomingMessage)
        {
            case "VX_TRIGGER_RECOGNITION":
                writer.WriteLine("VX_RECOGNITION_ACK");
                //writer.Flush();
                await RunRecognition();
                writer.WriteLine("VX_RECOGNITION_FIN");
                //writer.Flush();
                return true;
            case "VX_CONNECT_REQ":
                // Send the connection ack response back to Unity
                writer.WriteLine("VX_CONNECT_ACK");
                //writer.Flush();
                return true;
            default:
                Console.WriteLine("UNRECOGNISED MESSAGE SENT FROM UNITY");
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