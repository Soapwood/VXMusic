using VXMusic;
using VXMusic.Recognition.AudD;

WindowsAudioDeviceListener recorder = new WindowsAudioDeviceListener();

recorder.StartRecording();

XSOverlay xsOverlay = new XSOverlay();

xsOverlay.XSNotification("VXMusic is Listening...", "", 10);
Console.WriteLine("Recording started.");

// Wait for the capture to complete by monitoring the capture state
while (recorder.CurrentCaptureState != NAudio.CoreAudioApi.CaptureState.Stopped)
{
    Thread.Sleep(500);
}

recorder.StopRecording();

Console.WriteLine("Recording stopped. Audio saved.");
xsOverlay.XSNotification("Sounds great! Give me one sec..", "", 1);

var audDClient = new AudDClient();
var result = await audDClient.RunRecognition();

if (result.status == "error")
{
    xsOverlay.XSNotification("Recognition failed! Oh jaysus", "", 5);
    Console.WriteLine("Recognition failed! Oh jaysus");
} else if (result.result == null)
{
    xsOverlay.XSNotification("Oops, couldn't get that.", "Tech Tip: Have you tried turning up your World Volume?", 5);
    Console.WriteLine("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
} else
{
    xsOverlay.XSNotification(result.result.artist, result.result.title, 8);
    Console.WriteLine($"{result.result.artist}: {result.result.title}");
}
