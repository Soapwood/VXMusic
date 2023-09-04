using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

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
        // _logger = logger;
        // _logger.LogTrace("Creating WindowsAudioDeviceListener");
        
        // WasapiLoopbackCapture allows to capture all audio on default audio device
        // https://github.com/naudio/NAudio/blob/master/Docs/WasapiLoopbackCapture.md
        Capture = new WasapiLoopbackCapture();
        Capture.ShareMode = AudioClientShareMode.Shared; // Set the share mode
        Capture.WaveFormat = new WaveFormat(44100, 16, 1); // Shazam needs 1 channel
        RecordingTimeSeconds = 5;
    }

    public void StartRecording()
    {
        // _logger.LogTrace("Starting Recording...");
        try
        {
            Capture.DataAvailable += OnDataAvailable;
            
            // We want to write as WAV for lossless recording. Will need to convert to other formats from there.
            Writer = new WaveFileWriter(BufferFile, Capture.WaveFormat);

            Capture.StartRecording();
        }
        catch (Exception ex)
        {
            // _logger.LogError("Error starting recording: " + Environment.NewLine + ex.Message);
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
            // _logger.LogError("Error stopping recording: " + Environment.NewLine + ex.Message);
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
            // _logger.LogError("Error writing to file:" + Environment.NewLine + ex.Message);
        }
    }

}