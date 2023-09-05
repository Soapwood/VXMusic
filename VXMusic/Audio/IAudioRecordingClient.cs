namespace VXMusic.Audio;

public interface IAudioRecordingClient
{
    public void StartRecording();
    public void StopRecording();

    public int GetRecordingTimeSeconds();

    public bool IsCaptureStateStopped();
}