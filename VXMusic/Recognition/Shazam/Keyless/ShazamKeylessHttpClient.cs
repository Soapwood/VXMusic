using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Buffers.Binary;

namespace VXMusic.Recognition.Shazam.Keyless;

/// <summary>
/// HTTP client for keyless Shazam API that sends fingerprints directly to Shazam's public API.
/// </summary>
public class ShazamKeylessHttpClient : IHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger? _logger;

    private const string ApiTemplate = "https://amp.shazam.com/discovery/v5/{0}/{1}/iphone/-/tag/{2}/{3}";
    private static readonly Dictionary<string, string> BaseHeaders = new()
    {
        ["X-Shazam-Platform"] = "IPHONE",
        ["X-Shazam-AppVersion"] = "14.1.0",
        ["Accept"] = "*/*",
        ["Accept-Encoding"] = "gzip, deflate",
        ["User-Agent"] = "Shazam/3685 CFNetwork/1197 Darwin/20.0.0"
    };
    private static readonly Dictionary<string, string> DefaultParams = new()
    {
        ["sync"] = "true",
        ["webv3"] = "true",
        ["sampling"] = "true",
        ["connected"] = "",
        ["shazamapiversion"] = "v3",
        ["sharehub"] = "true",
        ["hubv5minorversion"] = "v5.1",
        ["hidelb"] = "true",
        ["video"] = "v3"
    };

    private readonly string _lang, _region, _timezone;
    public int MaxTimeSeconds { get; set; } = 15;

    public ShazamKeylessHttpClient(ILogger? logger = null, string lang = "en", string region = "US", string timezone = "America/New_York")
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _httpClient = new HttpClient(handler);
        _logger = logger;
        _lang = lang;
        _region = region;
        _timezone = timezone;
    }

    public async Task<IRecognitionApiClientResponse> GetArtist(string audioDataBase64)
    {
        // Convert base64 audio data to 16kHz mono for fingerprinting
        try
        {
            var audioBytes = Convert.FromBase64String(audioDataBase64);
            var samples = await LoadAndNormalizeFromBytes(audioBytes);
            return await RecognizeFromSamples(samples);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error processing audio data: {ex}");
            return new ShazamResponse
            {
                Status = Status.ApiError,
                Result = null
            };
        }
    }

    public async Task<IRecognitionApiClientResponse> GetArtist(byte[] audioBytes)
    {
        try
        {
            var samples = await LoadAndNormalizeFromBytes(audioBytes);
            return await RecognizeFromSamples(samples);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error processing audio bytes: {ex}");
            return new ShazamResponse
            {
                Status = Status.ApiError,
                Result = null
            };
        }
    }

    private async Task<IRecognitionApiClientResponse> RecognizeFromSamples(List<short> samples)
    {
        try
        {
            _logger?.LogInformation($"Starting signature generation with {samples.Count} samples");
            
            var sigGen = new SignatureGenerator(_logger) { MaxTimeSeconds = MaxTimeSeconds };
            sigGen.FeedInput(samples);

            var sig = sigGen.GetNextSignature();
            if (sig == null)
            {
                _logger?.LogWarning("SignatureGenerator returned null - not enough audio data or processing failed");
                return new ShazamResponse
                {
                    Status = Status.RecordingError,
                    Result = null
                };
            }

            _logger?.LogInformation($"Signature generated: {sig.NumberSamples} samples processed, {sig.FrequencyBandToSoundPeaks.Count} frequency bands, {sig.FrequencyBandToSoundPeaks.Values.Sum(x => x.Count)} total peaks");
            
            if (sig.FrequencyBandToSoundPeaks.Values.Sum(x => x.Count) == 0)
            {
                _logger?.LogWarning("No frequency peaks detected - audio may be silent or low quality");
                return new ShazamResponse
                {
                    Status = Status.NoMatches,
                    Result = null
                };
            }

            var json = await SendRecognizeRequestAsync(sig);
            return await ParseShazamResponse(json);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error during recognition: {ex}");
            return new ShazamResponse
            {
                Status = Status.ApiError,
                Result = null
            };
        }
    }

    private async Task<List<short>> LoadAndNormalizeFromBytes(byte[] audioData)
    {
        _logger?.LogDebug($"ShazamKeylessHttpClient.LoadAndNormalizeFromBytes: start, length {audioData.Length}");
        
        try
        {
        using var ms = new MemoryStream(audioData);
            using var reader = new WaveFileReader(ms);
            
            _logger?.LogInformation($"Original audio format: {reader.WaveFormat.SampleRate}Hz, {reader.WaveFormat.BitsPerSample}bit, {reader.WaveFormat.Channels}ch, Duration: {reader.TotalTime.TotalSeconds:F2}s");
            
            // Shazam expects 16kHz mono 16-bit
            var outFormat = new WaveFormat(16000, 16, 1);
            
            // Use high-quality resampling for better fingerprinting
            using var resampler = new MediaFoundationResampler(reader, outFormat)
            {
                ResamplerQuality = 60 // High quality
            };
            
            _logger?.LogDebug($"Resampling from {reader.WaveFormat.SampleRate}Hz to {outFormat.SampleRate}Hz");

            var buffer = new byte[outFormat.AverageBytesPerSecond];
            var samples = new List<short>();
            int bytesRead;
            int totalBytesRead = 0;

            await Task.Run(() =>
            {
                while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalBytesRead += bytesRead;
                    for (int i = 0; i < bytesRead; i += 2)
                    {
                        if (i + 1 < bytesRead)
                            samples.Add(BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(i, 2)));
                    }
                }
            });

            double durationSeconds = samples.Count / 16000.0;
            _logger?.LogInformation($"Audio processed: {samples.Count} samples, {durationSeconds:F2}s duration, {totalBytesRead} bytes read");
            
            // Validate minimum duration for Shazam recognition
            if (samples.Count < 16000) // Less than 1 second of audio
            {
                _logger?.LogWarning($"Short audio clip: only {durationSeconds:F2}s, may not be sufficient for recognition");
            }
            
            if (samples.Count < 48000) // Less than 3 seconds - warn but continue
            {
                _logger?.LogWarning($"Audio duration {durationSeconds:F2}s is quite short. Shazam typically needs 3-10s for reliable recognition");
            }
            
            return samples;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error processing audio: {ex}");
            throw;
        }
    }

    private async Task<string> SendRecognizeRequestAsync(DecodedMessage sig)
    {
        _logger?.LogDebug("ShazamKeylessHttpClient.SendRecognizeRequestAsync: start");

        var uuidA = Guid.NewGuid().ToString().ToUpper();
        var uuidB = Guid.NewGuid().ToString().ToUpper();
        var uri = string.Format(ApiTemplate, _lang, _region, uuidA, uuidB);

        // Calculate accurate duration based on the signature
        var durationMs = (int)(sig.NumberSamples / (double)sig.SampleRateHz * 1000);
        _logger?.LogInformation($"Signature duration: {durationMs}ms ({sig.NumberSamples} samples at {sig.SampleRateHz}Hz)");
        
        // Validate signature before sending
        var signatureUri = sig.EncodeToUri();
        _logger?.LogDebug($"Signature URI length: {signatureUri.Length} characters");
        
        if (signatureUri.Length < 100) // Too short signature suggests no meaningful data
        {
            _logger?.LogWarning($"Signature seems too short ({signatureUri.Length} chars) - may indicate poor audio quality");
        }
        
        var payload = new
        {
            timezone = _timezone,
            signature = new
            {
                uri = signatureUri,
                samplems = durationMs
            },
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            context = new { },
            geolocation = new { }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, uri);
        foreach (var kv in BaseHeaders)
            req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

        req.Headers.AcceptLanguage.ParseAdd(_lang);
        req.Content = new StringContent(
            JsonConvert.SerializeObject(payload),
            Encoding.UTF8,
            "application/json"
        );

        // Append default params to the query string
        var query = string.Join("&",
            DefaultParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        req.RequestUri = new Uri(req.RequestUri + "?" + query);

        _logger?.LogDebug($"Sending request to: {req.RequestUri}");
        _logger?.LogDebug($"Request payload: {JsonConvert.SerializeObject(payload, Formatting.Indented)}");
        
        var resp = await _httpClient.SendAsync(req);
        _logger?.LogDebug($"Response status: {resp.StatusCode}");
        
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync();
        _logger?.LogDebug($"Response body: {body}");
        _logger?.LogDebug("ShazamKeylessHttpClient.SendRecognizeRequestAsync: success");
        return body;
    }

    private async Task<IRecognitionApiClientResponse> ParseShazamResponse(string json)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<dynamic>(json);
            
            // Check if we have matches
            if (response?.matches == null || response.matches.Count == 0)
            {
                return new ShazamResponse() { Status = Status.NoMatches };
            }

            // Track data is at the root level, not inside matches
            var track = response?.track;
            
            if (track == null)
            {
                return new ShazamResponse() { Status = Status.NoMatches };
            }

            string albumArtBase64 = ConvertShazamAlbumArtUrlToBase64(track.images?.coverart?.ToString());

            // Parse track information
            var result = new Result
            {
                Artist = track.subtitle?.ToString(),
                Title = track.title?.ToString(),
                Album = GetMetadataValue(track, "Album"),
                ReleaseDate = GetMetadataValue(track, "Released"),
                Label = GetMetadataValue(track, "Label"),
                SongLink = track.url?.ToString(),
                AlbumArt = albumArtBase64 ?? ""
            };

            return new ShazamResponse
            {
                Status = Status.Success,
                Result = result
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error parsing Shazam response: {ex}");
            return new ShazamResponse
            {
                Status = Status.ApiError,
                Result = null
            };
        }
    }

    private async Task<string> ConvertShazamAlbumArtUrlToBase64(string albumArtUrl)
    {
        string albumArtBase64 = null;
        if (!string.IsNullOrEmpty(albumArtUrl))
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    var imageBytes = await httpClient.GetByteArrayAsync(albumArtUrl);
                    albumArtBase64 = Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to download album art from {albumArtUrl}: {ex.Message}");
            }
        }

        return albumArtBase64;
    }

    private string? GetMetadataValue(dynamic track, string title)
    {
        try
        {
            if (track?.sections?[0]?.metadata != null)
            {
                foreach (var metadata in track.sections[0].metadata)
                {
                    if (metadata?.title?.ToString() == title)
                    {
                        return metadata?.text?.ToString();
                    }
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }
        return null;
    }

    public void SetApiKey(string apiKey)
    {
        // No-op in keyless mode - API keys are not used
        _logger?.LogDebug("SetApiKey called in keyless mode - ignoring");
    }

    public async Task<bool> TestConnection()
    {
        // In keyless mode, we assume the connection is available
        // We could potentially test with a lightweight request but it's complex
        _logger?.LogDebug("TestConnection called in keyless mode - assuming available");
        return await Task.FromResult(true);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}