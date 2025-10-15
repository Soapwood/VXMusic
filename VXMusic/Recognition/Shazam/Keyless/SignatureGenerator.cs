using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.Extensions.Logging;

namespace VXMusic.Recognition.Shazam.Keyless;

/// <summary>
/// Core FFT+peak‐finding signature generator.
/// </summary>
public class SignatureGenerator
{
    private readonly ILogger? _logger;
    private const int WindowSize = 2048;
    private const int StepSize = 128;
    private static readonly double[] HanningWindow =
        Window.Hann(2050).Skip(1).Take(WindowSize).ToArray();

    private readonly List<short> _inputBuffer = new();
    private int _samplesProcessed;
    private RingBuffer<short> _ringBufferSamples = new(WindowSize, 0);
    private RingBuffer<double[]> _fftOutputs = new(256, new double[1025]);
    private RingBuffer<double[]> _spreadFfts = new(256, new double[1025]);
    private DecodedMessage _nextSignature = new() { SampleRateHz = 16000, NumberSamples = 0 };

    public int SamplesProcessed => _samplesProcessed;
    public int MaxPeaks { get; set; } = 255;
    public double MaxTimeSeconds { get; set; } = 15.0; // Increase to capture more audio for better recognition

    public SignatureGenerator(ILogger? logger = null)
    {
        _logger = logger;
    }

    public void FeedInput(IEnumerable<short> samples)
    {
        try
        {
            _logger?.LogDebug("SignatureGenerator.FeedInput: start");
            _inputBuffer.AddRange(samples);
            _logger?.LogDebug($"SignatureGenerator.FeedInput: buffer size {_inputBuffer.Count}");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"SignatureGenerator.FeedInput: exception {ex}");
            throw;
        }
    }

    public DecodedMessage? GetNextSignature()
    {
        try
        {
            _logger?.LogDebug("SignatureGenerator.GetNextSignature: start");

            if (_inputBuffer.Count - _samplesProcessed < StepSize)
            {
                _logger?.LogDebug($"SignatureGenerator.GetNextSignature: insufficient data, available: {_inputBuffer.Count - _samplesProcessed}, needed: {StepSize}");
                return null;
            }

            // Process until we have enough data or hit time limit
            var totalAvailable = _inputBuffer.Count - _samplesProcessed;
            _logger?.LogDebug($"Processing audio: {totalAvailable} samples available, need {StepSize} minimum");
            
            while (_inputBuffer.Count - _samplesProcessed >= StepSize
                && (_nextSignature.NumberSamples / (double)_nextSignature.SampleRateHz < MaxTimeSeconds
                    || _nextSignature.FrequencyBandToSoundPeaks.Values.Sum(l => l.Count) < MaxPeaks))
            {
                var chunk = _inputBuffer
                    .Skip(_samplesProcessed)
                    .Take(StepSize)
                    .ToArray();

                ProcessInput(chunk);
                _samplesProcessed += StepSize;
                
                var currentTime = _nextSignature.NumberSamples / (double)_nextSignature.SampleRateHz;
                var totalPeaks = _nextSignature.FrequencyBandToSoundPeaks.Values.Sum(l => l.Count);
                _logger?.LogDebug($"Processed chunk: {currentTime:F2}s, {totalPeaks} peaks so far");
            }

            var result = _nextSignature;
            var finalDuration = result.NumberSamples / (double)result.SampleRateHz;
            var finalPeaks = result.FrequencyBandToSoundPeaks.Values.Sum(l => l.Count);
            _logger?.LogInformation($"Signature generation complete: {finalDuration:F2}s processed, {finalPeaks} peaks detected");
            
            Reset();
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"SignatureGenerator.GetNextSignature: exception {ex}");
            throw;
        }
    }

    private void ProcessInput(short[] samples)
    {
        try
        {
            _nextSignature.NumberSamples += samples.Length;
            DoFft(samples);
            DoPeakSpreading();
            if (_spreadFfts.NumWritten >= 46)
                DoPeakRecognition();
        }
        catch (Exception ex)
        {
            _logger?.LogError($"SignatureGenerator.ProcessInput: exception {ex}");
            throw;
        }
    }

    private void DoFft(short[] batch)
    {
        try
        {
            foreach (var s in batch)
            {
                _ringBufferSamples.Append(s);
            }

            // extract + window + FFT
            var wrap = _ringBufferSamples.Position;
            var excerpt = new double[WindowSize];
            for (int i = 0; i < WindowSize; i++)
                excerpt[i] = _ringBufferSamples[(wrap + i) % WindowSize] * HanningWindow[i];

            var complex = excerpt.Select(d => new Complex(d, 0)).ToArray();
            Fourier.Forward(complex, FourierOptions.Matlab);

            // power spectrum
            var power = new double[1025];
            for (int i = 0; i < power.Length; i++)
            {
                power[i] = Math.Max((complex[i].Magnitude * complex[i].Magnitude) / (1 << 17), 1e-10);
            }

            _fftOutputs.Append(power);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"SignatureGenerator.DoFft: exception {ex}");
            throw;
        }
    }

    private void DoPeakSpreading()
    {
        try
        {
            // 1) Grab the last FFT output, with safe wrap
            int fftBufSize = _fftOutputs.BufferSize;
            int lastPos = (_fftOutputs.Position - 1 + fftBufSize) % fftBufSize;
            var last = _fftOutputs[lastPos];

            // 2) Clone for frequency‐domain spreading
            var spread = (double[])last.Clone();

            // freq‐domain spreading
            for (int i = 0; i < spread.Length; i++)
            {
                if (i <= spread.Length - 3)
                    spread[i] = spread.Skip(i).Take(3).Max();
            }

            // 3) Time‐domain spreading with safe index wrap
            int spreadBufSize = _spreadFfts.BufferSize;
            for (int i = 0; i < spread.Length; i++)
            {
                double m = spread[i];
                foreach (int offset in new[] { -1, -3, -6 })
                {
                    int rawIndex = _spreadFfts.Position + offset;
                    int wrapIndex = ((rawIndex % spreadBufSize) + spreadBufSize) % spreadBufSize;
                    var prev = _spreadFfts[wrapIndex];
                    m = Math.Max(m, prev[i]);
                    prev[i] = m;
                }
            }
            _spreadFfts.Append(spread);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"SignatureGenerator.DoPeakSpreading: exception\\n{ex}");
            throw;
        }
    }

    private void DoPeakRecognition()
    {
        try
        {
            int fftBufSize = _fftOutputs.BufferSize;
            int idx46 = (_fftOutputs.Position - 46 + fftBufSize) % fftBufSize;
            var fft46 = _fftOutputs[idx46];

            int spreadBufSize = _spreadFfts.BufferSize;
            int idx49 = (_spreadFfts.Position - 49 + spreadBufSize) % spreadBufSize;
            var sp49 = _spreadFfts[idx49];

            int fftPassNumber = _spreadFfts.NumWritten - 46;

            for (int bin = 10; bin < 1015; bin++)
            {
                if (fft46[bin] < 1.0 / 64 || fft46[bin] < sp49[bin - 1]) continue;

                double maxNeighborIn49 = new[] { -10, -7, -4, -3, 1, 4, 7 }
                    .Select(off => sp49[bin + off]).Max();
                if (fft46[bin] <= maxNeighborIn49) continue;

                double maxTimeNeighbor = maxNeighborIn49;
                foreach (int off in new[] { -53, -45 }
                    .Concat(Enumerable.Range(165, 36).Where((_, i) => i % 7 == 0))
                    .Concat(Enumerable.Range(214, 36).Where((_, i) => i % 7 == 0)))
                {
                    int j = (_spreadFfts.Position + off + spreadBufSize) % spreadBufSize;
                    maxTimeNeighbor = Math.Max(maxTimeNeighbor, _spreadFfts[j][bin - 1]);
                }
                if (fft46[bin] <= maxTimeNeighbor) continue;

                double peakMag = Math.Log(Math.Max(1.0 / 64, fft46[bin])) * 1477.3 + 6144;
                double magBefore = Math.Log(Math.Max(1.0 / 64, fft46[bin - 1])) * 1477.3 + 6144;
                double magAfter = Math.Log(Math.Max(1.0 / 64, fft46[bin + 1])) * 1477.3 + 6144;
                double var1 = peakMag * 2 - magBefore - magAfter;
                if (var1 <= 0) throw new InvalidOperationException("peak_variation_1 <= 0");

                double var2 = (magAfter - magBefore) * 32 / var1;
                int correctedBin = (int)(bin * 64 + var2);
                double freqHz = correctedBin * (16000.0 / 2.0 / 1024.0 / 64.0);

                FrequencyBand band;
                if (freqHz < 250) continue;
                else if (freqHz < 520) band = FrequencyBand.band_250_520;
                else if (freqHz < 1450) band = FrequencyBand.band_520_1450;
                else if (freqHz < 3500) band = FrequencyBand.band_1450_3500;
                else if (freqHz <= 5500) band = FrequencyBand.band_3500_5500;
                else continue;

                if (!_nextSignature.FrequencyBandToSoundPeaks.ContainsKey(band))
                    _nextSignature.FrequencyBandToSoundPeaks[band] = new List<FrequencyPeak>();

                _nextSignature.FrequencyBandToSoundPeaks[band].Add(
                    new FrequencyPeak(fftPassNumber, (int)peakMag, correctedBin, 16000)
                );
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"SignatureGenerator.DoPeakRecognition: exception {ex}");
            throw;
        }
    }

    private void Reset()
    {            
        _nextSignature = new DecodedMessage
        {
            SampleRateHz = 16000,
            NumberSamples = 0,
            FrequencyBandToSoundPeaks = new Dictionary<FrequencyBand, List<FrequencyPeak>>()
        };

        _ringBufferSamples = new RingBuffer<short>(WindowSize, 0);
        _fftOutputs = new RingBuffer<double[]>(256, new double[1025]);
        _spreadFfts = new RingBuffer<double[]>(256, new double[1025]);
    }
}