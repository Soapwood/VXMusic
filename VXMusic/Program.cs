using VXMusic;
using VXMusic.Recognition.AudD;

WindowsAudioDeviceListener recorder = new WindowsAudioDeviceListener();

recorder.StartRecording();

XSOverlay.XSNotification("VXMusic is Listening...", "", 10);
Console.WriteLine("Recording started.");

// Wait for the capture to complete by monitoring the capture state
while (recorder.CurrentCaptureState != NAudio.CoreAudioApi.CaptureState.Stopped)
{
    Thread.Sleep(500);
}

recorder.StopRecording();

Console.WriteLine("Recording stopped. Audio saved.");
XSOverlay.XSNotification("Sounds great! Give me one sec..", "", 1);

var audDClient = new AudDClient();
var result = await audDClient.RunRecognition();

if (result.status == "error")
{
    XSOverlay.XSNotification("Recognition failed! Oh jaysus", "", 5);
    Console.WriteLine("Recognition failed! Oh jaysus");
} else if (result.result == null)
{
    XSOverlay.XSNotification("Oops, couldn't get that.", "Tech Tip: Have you tried turning up your World Volume?", 5);
    Console.WriteLine("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
} else
{
    XSOverlay.XSNotification(result.result.artist, result.result.title, 8);
    Console.WriteLine($"{result.result.artist}: {result.result.title}");
}
