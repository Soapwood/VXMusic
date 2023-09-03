namespace VXMusic;

public interface IRecognitionClient
{
    public Task<IRecognitionApiClientResponse> RunRecognition();
}