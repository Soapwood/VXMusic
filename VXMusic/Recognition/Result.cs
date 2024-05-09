namespace VXMusic;

public class Result
{
    public string? Artist { get; set; }
    public string? Title { get; set; }
    public string? Album { get; set; }
    public string? ReleaseDate { get; set; }
    public string? Label { get; set; }
    public string? SongLink { get; set; }
    public string? AlbumArt { get; set; }

}

public enum Status
{
    Success,
    Error,
    NoMatches,
    RecordingError
}

public interface IRecognitionApiClientResponse
{
    public Status Status { get; }
    public Result Result { get; }
}