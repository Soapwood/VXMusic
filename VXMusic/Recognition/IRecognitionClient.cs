namespace VXMusic;

public interface IRecognitionClient
{
    public Task<IApiClientResponse> RunRecognition();
}