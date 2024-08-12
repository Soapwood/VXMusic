namespace VXMusic.Audio.Device;

public class AudioDevice
{
    public string DeviceFriendlyName { get; set; }
    public bool IsDefaultAudioDevice { get; set; }

    public override string ToString()
    {
        return $"Device Name: {DeviceFriendlyName} | IsDefault: {IsDefaultAudioDevice}";
    }

    public static AudioDevice Default = new AudioDevice() { DeviceFriendlyName = "Default (Everything)", IsDefaultAudioDevice = true };
}