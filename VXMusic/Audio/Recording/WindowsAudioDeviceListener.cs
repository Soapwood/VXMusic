using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VXMusic.Audio;

namespace VXMusic.Audio.Recording;

public class WindowsAudioDeviceListener : IAudioRecordingClient
{
    private ILogger<WindowsAudioDeviceListener> _logger;

    private WasapiLoopbackCapture? Capture;
    private WaveFileWriter Writer;
    private readonly String BufferFile = "output.wav";

    private int RecordingTimeSeconds;

    private CaptureState CurrentCaptureState => Capture.CaptureState;

    public WindowsAudioDeviceListener(ILogger<WindowsAudioDeviceListener> logger)
    {
        _logger = logger;
        _logger.LogTrace("Creating WindowsAudioDeviceListener");

        CreateCaptureInstance();
        RecordingTimeSeconds = 5;
    }
    
    public int GetRecordingTimeSeconds()
    {
        return RecordingTimeSeconds;
    }

    public bool IsCaptureStateStopped()
    {
        // Wait for the capture to complete by monitoring the capture state
        return CurrentCaptureState == CaptureState.Stopped;
    }
    
    private void CreateCaptureInstance()
    {
        // WasapiLoopbackCapture allows to capture all audio on default audio device
        // https://github.com/naudio/NAudio/blob/master/Docs/WasapiLoopbackCapture.md
        Capture = new WasapiLoopbackCapture();
        Capture.ShareMode = AudioClientShareMode.Shared; // Set the share mode
        Capture.WaveFormat = new WaveFormat(44100, 16, 1); // Shazam needs 1 channel
    }

    public void StartRecording()
    {
        // Recreate WasapiLoopbackCapture object if it has been disposed.
        if (Capture == null)
            CreateCaptureInstance();
        
        try
        {
            Capture.DataAvailable += OnDataAvailable;
            
            // We want to write as WAV for lossless recording. Will need to convert to other formats from there.
            Writer = new WaveFileWriter(BufferFile, Capture.WaveFormat);

            Capture.StartRecording();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error starting recording: " + Environment.NewLine + ex.Message);
        }
        _logger.LogTrace("Recording Started.");

    }

    public void StopRecording()
    {
        try
        {
            Capture?.StopRecording();
            Writer?.Dispose();
            Capture?.Dispose();
            
            // Now that we're creating the Audio Listener as a service, we want to keep the service alive in memory.
            // However, we can't purge the audioBuffer publically. We need to dispose of it here and recreate it again.
            Capture = null;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error stopping recording: " + Environment.NewLine + ex.Message);
        }
        
        _logger.LogTrace("Recording stopped.");
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
            _logger.LogError("Error writing to file:" + Environment.NewLine + ex.Message);
        }
    }

}