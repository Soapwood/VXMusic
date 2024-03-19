namespace VXMusic;

public interface IRecognitionClient
{
    public Task<IRecognitionApiClientResponse> RunRecognition();

    public void SetByoApiKey(string byoApiKey);
}