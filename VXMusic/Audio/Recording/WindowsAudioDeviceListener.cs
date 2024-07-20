using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VXMusic.Audio;

namespace VXMusic.Audio.Recording;

public class WindowsAudioDeviceListener : IAudioRecordingClient
{
    private readonly IServiceProvider _serviceProvider;
    private ILogger<WindowsAudioDeviceListener> _logger;

    private WasapiLoopbackCapture? _capture;
    private WaveFileWriter? _writer;
    
    static string BufferFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualXtensions", "VXMusic", "Cache", "output.wav");

    private int _recordingTimeSeconds;

    private CaptureState CurrentCaptureState => _capture.CaptureState;

    public WindowsAudioDeviceListener(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<WindowsAudioDeviceListener>)) 
            as ILogger<WindowsAudioDeviceListener> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogTrace("Creating WindowsAudioDeviceListener");

        CreateCaptureInstance();
        _recordingTimeSeconds = 5;

        CreateCacheDirectoryIfDoesNotExist();
    }
    
    public int GetRecordingTimeSeconds()
    {
        return _recordingTimeSeconds;
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
        _capture = new WasapiLoopbackCapture();
        _capture.ShareMode = AudioClientShareMode.Shared; // Set the share mode
        _capture.WaveFormat = new WaveFormat(44100, 16, 1); // Shazam needs 1 channel
    }

    public void StartRecording()
    {
        // Recreate WasapiLoopbackCapture object if it has been disposed.
        if (_capture == null)
            CreateCaptureInstance();
        
        try
        {
            _capture.DataAvailable += OnDataAvailable;
            
            // We want to write as WAV for lossless recording. Will need to convert to other formats from there.
            _writer = new WaveFileWriter(BufferFile, _capture.WaveFormat);

            _capture.StartRecording();
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
            _capture?.StopRecording();
            _writer?.Dispose();
            _capture?.Dispose();
            
            // Now that we're creating the Audio Listener as a service, we want to keep the service alive in memory.
            // However, we can't purge the audioBuffer publically. We need to dispose of it here and recreate it again.
            _capture = null;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error stopping recording: " + Environment.NewLine + ex.Message);
        }
        
        _logger.LogTrace("Recording stopped.");
    }

    private void CreateCacheDirectoryIfDoesNotExist()
    {
        var bufferFileRoot = Directory.GetParent(BufferFile).ToString();
        
        if(!Directory.Exists(bufferFileRoot))
        {
            Directory.CreateDirectory(bufferFileRoot);
        }
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        try
        {
            _writer?.Write(e.Buffer, 0, e.BytesRecorded);
            
            // Limits to 10 second of recording. TODO Inject this time limit 
            if (_writer.Position > _capture.WaveFormat.AverageBytesPerSecond * _recordingTimeSeconds)
                _capture.StopRecording();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error writing to file:" + Environment.NewLine + ex.Message);
        }
    }

}