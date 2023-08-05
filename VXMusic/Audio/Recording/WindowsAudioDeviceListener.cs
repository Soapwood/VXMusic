using NAudio.Lame;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Diagnostics;

namespace VXMusic;

public class WindowsAudioDeviceListener
{
    private WasapiLoopbackCapture Capture;
    private WaveFileWriter Writer;
    private readonly String BufferFile = "output.wav";
    
    public int RecordingTimeSeconds;

    public CaptureState CurrentCaptureState => Capture.CaptureState;

    public WindowsAudioDeviceListener()
    {
        // WasapiLoopbackCapture allows to capture all audio on default audio device
        // https://github.com/naudio/NAudio/blob/master/Docs/WasapiLoopbackCapture.md
        Capture = new WasapiLoopbackCapture();
        Capture.ShareMode = AudioClientShareMode.Shared; // Set the share mode
        Capture.WaveFormat = new WaveFormat(44100, 16, 1); // Shazam needs 1 channel
        RecordingTimeSeconds = 5;
    }

    public void StartRecording()
    {
        try
        {
            Capture.DataAvailable += OnDataAvailable;
            
            // We want to write as WAV for lossless recording. Will need to convert to other formats from there.
            Writer = new WaveFileWriter(BufferFile, Capture.WaveFormat);

            Capture.StartRecording();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("Error starting recording: " + ex.Message);
        }
    }

    public void StopRecording()
    {
        try
        {
            Capture?.StopRecording();
            Writer?.Dispose();
            Capture?.Dispose();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("Error stopping recording: " + ex.Message);
        }
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        try
        {
            Writer?.Write(e.Buffer, 0, e.BytesRecorded);
            
            // Limits to 10 second of recording. TODO Inject this time limit 
            if (Writer.Position > Capture.WaveFormat.AverageBytesPerSecond * RecordingTimeSeconds)
                Capture.StopRecording();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("Error writing to file: " + ex.Message);
        }
    }

}