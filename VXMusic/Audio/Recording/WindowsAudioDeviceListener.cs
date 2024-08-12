using System.Collections.ObjectModel;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VXMusic.Audio;
using VXMusic.Audio.Device;

namespace VXMusic.Audio.Recording;

public class WindowsAudioDeviceListener : IAudioRecordingClient
{
    private readonly IServiceProvider _serviceProvider;
    private ILogger<WindowsAudioDeviceListener> _logger;

    private WasapiLoopbackCapture? _capture;
    private WasapiCapture? _audioDeviceCapture;
    private WaveFileWriter? _writer;

    private int _recordingRate = 44100;
    private int _recordingBits = 16;
    private int _recordingChannels = 1;
    
    static string BufferFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VirtualXtensions", "VXMusic", "Cache", "output.wav");

    private int _recordingTimeSeconds;

    //private CaptureState CurrentCaptureState => SelectedAudioDevice.IsDefaultAudioDevice ? _capture.CaptureState : _audioDeviceCapture.CaptureState;

    private CaptureState CurrentCaptureState
    {
        get
        {
            if (IsSelectedAudioDeviceDefault)
            {
                return _capture.CaptureState;
            }
            else
            {
                return _audioDeviceCapture.CaptureState;
            }
        }
    }
    
    
    //private static ObservableCollection<MMDevice> AvailableMMDevices;
    public static List<AudioDevice> AvailableAudioDevices = new List<AudioDevice>();

    //public static MMDeviceCollection AvailableAudioDevices;
    public static AudioDevice SelectedAudioDevice;

    public static bool IsSelectedAudioDeviceDefault => SelectedAudioDevice.IsDefaultAudioDevice;

    public WindowsAudioDeviceListener(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<WindowsAudioDeviceListener>))
                      as ILogger<WindowsAudioDeviceListener> ??
                  throw new ApplicationException("A logger must be created in service provider.");

        _logger.LogTrace("Creating WindowsAudioDeviceListener");

        InitialiseAudioDevices();
        
        SelectedAudioDevice = AudioDevice.Default;

        _recordingTimeSeconds = 5;

        CreateCacheDirectoryIfDoesNotExist();
    }

    public void InitialiseAudioDevices()
    {
        _logger.LogInformation("Initialising Audio Devices.");

        MMDeviceCollection availableMMDevices = GetWindowsRenderDevices();

        AvailableAudioDevices.Add(AudioDevice.Default);

        AvailableAudioDevices.AddRange(availableMMDevices.Cast<MMDevice>()
            .Select(device => new AudioDevice
            {
                DeviceFriendlyName = device.FriendlyName,
                IsDefaultAudioDevice = false
            })
            .ToList());

        _logger.LogDebug($"Found the following Audio Devices:");
        AvailableAudioDevices.ForEach(device => _logger.LogDebug($"{device}"));
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

    private void CreateDefaultWasapiLoopbackCaptureInstance()
    {
        // WasapiLoopbackCapture allows to capture all audio on default audio device
        // https://github.com/naudio/NAudio/blob/master/Docs/WasapiLoopbackCapture.md
        _capture = new WasapiLoopbackCapture();
        _capture.ShareMode = AudioClientShareMode.Shared; // Set the share mode
        _capture.WaveFormat = new WaveFormat(_recordingRate, _recordingBits, _recordingChannels); // Shazam needs 1 channel

        _capture.DataAvailable += OnDataAvailable;

        _writer = new WaveFileWriter(BufferFile, _capture.WaveFormat);

        _capture.StartRecording();
    }

    private void CreateCaptureInstanceWithSelectedAudioDevice(AudioDevice audioDevice)
    {
        // Capture specific device
        var availableRenderDevices = GetWindowsRenderDevices();

        MMDevice? mmDevice;

        // Set the selected device if there are any available devices
        try
        {
            mmDevice = availableRenderDevices.FirstOrDefault(d =>
                d.FriendlyName.Equals(SelectedAudioDevice.DeviceFriendlyName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception e)
        {
            _logger.LogError("Could not find available Audio Device from Friendly Name. Resorting to Default.");
            SelectedAudioDevice = AudioDevice.Default;
            CreateDefaultWasapiLoopbackCaptureInstance();
            return;
        }

        if (mmDevice == null)
        {
            _logger.LogError(
                "Could not find available Audio Device, returned MMDevice was null. Resorting to Default.");
            SelectedAudioDevice = AudioDevice.Default;
            CreateDefaultWasapiLoopbackCaptureInstance();
            return;
        }

        _audioDeviceCapture = new WasapiCapture(mmDevice);

        _audioDeviceCapture.ShareMode = AudioClientShareMode.Shared; // Set the share mode
        _audioDeviceCapture.WaveFormat =
            new WaveFormat(_recordingRate, _recordingBits, _recordingChannels); // Shazam needs 1 channel

        _audioDeviceCapture.DataAvailable += OnDataAvailable;

        _writer = new WaveFileWriter(BufferFile, _audioDeviceCapture.WaveFormat);

        try
        {
            _audioDeviceCapture.StartRecording();
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to start recording: {e.Message}");
        }
        
    }

    static MMDeviceCollection GetWindowsRenderDevices()
    {
        // Create an MMDeviceEnumerator to get the audio devices
        var enumerator = new MMDeviceEnumerator();
        // Get the audio endpoint collection for both render and capture devices
        return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
    }

    public void StartRecording()
    {
        _logger.LogTrace("Starting Recording... setting up devices.");
        try
        {
            if (IsSelectedAudioDeviceDefault)
            {
                _logger.LogTrace("Default Audio Device is selected. Creating WasapiLoopbackCapture.");
                CreateDefaultWasapiLoopbackCaptureInstance();
            }
            else
            {
                _logger.LogTrace($"Selected Audio Device {SelectedAudioDevice}.");
                CreateCaptureInstanceWithSelectedAudioDevice(SelectedAudioDevice);
            }

            _logger.LogTrace("Recording Started.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error starting recording: " + Environment.NewLine + ex.Message);
        }
    }

    public void StopRecording()
    {
        try
        {
            if (IsSelectedAudioDeviceDefault)
            {
                _capture?.StopRecording();
                _capture?.Dispose();
                _capture = null;
            }
            else
            {
                _audioDeviceCapture?.StopRecording();
                _audioDeviceCapture.Dispose();
                _audioDeviceCapture = null;
            }

            _writer?.Dispose();
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

        if (!Directory.Exists(bufferFileRoot))
        {
            Directory.CreateDirectory(bufferFileRoot);
        }
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        try
        {
            _writer?.Write(e.Buffer, 0, e.BytesRecorded);

            var averageBytesPerSecond = IsSelectedAudioDeviceDefault
                ? _capture.WaveFormat.AverageBytesPerSecond
                : _audioDeviceCapture.WaveFormat.AverageBytesPerSecond;
            
            if (_writer.Position > averageBytesPerSecond * _recordingTimeSeconds)
            {
                if (IsSelectedAudioDeviceDefault)
                    _capture.StopRecording();
                else
                    _audioDeviceCapture.StopRecording();
            }
                
        }
        catch (Exception ex)
        {
            _logger.LogError("Error writing to file:" + Environment.NewLine + ex.Message);
        }
    }
}