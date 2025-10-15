namespace VXMusic;

public interface IRecognitionClient
{
    public Task<IRecognitionApiClientResponse> RunRecognition();
    
    public Task<bool> TestApiConnection();
}