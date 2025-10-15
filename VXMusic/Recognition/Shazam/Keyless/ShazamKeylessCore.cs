using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace VXMusic.Recognition.Shazam.Keyless;

/// <summary>
/// Frequency bands used by Shazam.
/// </summary>
public enum FrequencyBand
{
    band_0_250 = -1,
    band_250_520 = 0,
    band_520_1450 = 1,
    band_1450_3500 = 2,
    band_3500_5500 = 3
}

/// <summary>
/// Holds one frequency‐peak detection.
/// </summary>
public class FrequencyPeak
{
    public int FftPassNumber { get; }
    public int PeakMagnitude { get; }
    public int CorrectedPeakFrequencyBin { get; }
    public int SampleRateHz { get; }

    public FrequencyPeak(int fftPassNumber, int peakMagnitude, int correctedPeakFrequencyBin, int sampleRateHz)
    {
        FftPassNumber = fftPassNumber;
        PeakMagnitude = peakMagnitude;
        CorrectedPeakFrequencyBin = correctedPeakFrequencyBin;
        SampleRateHz = sampleRateHz;
    }

    public double GetFrequencyHz() =>
        CorrectedPeakFrequencyBin * (SampleRateHz / 2.0 / 1024.0 / 64.0);

    public double GetAmplitudePcm() =>
        Math.Sqrt(
            Math.Exp((PeakMagnitude - 6144.0) / 1477.3) * (1 << 17) / 2.0
        ) / 1024.0;

    public double GetSeconds() =>
        (FftPassNumber * 128.0) / SampleRateHz;
}

/// <summary>
/// A simple ring buffer implementation for audio processing.
/// </summary>
public class RingBuffer<T>
{
    private readonly T[] _buffer;
    public int Position { get; set; }
    public int NumWritten { get; set; }
    public int BufferSize => _buffer.Length;

    public RingBuffer(int bufferSize, T defaultValue = default!)
    {
        _buffer = Enumerable.Repeat(defaultValue, bufferSize).ToArray();
        Position = 0;
        NumWritten = 0;
    }

    public T this[int idx]
    {
        get => _buffer[idx];
        set => _buffer[idx] = value;
    }

    public void Append(T value)
    {
        _buffer[Position] = value;
        Position = (Position + 1) % BufferSize;
        NumWritten++;
    }
}

/// <summary>
/// C struct mapping for the 48‐byte signature header.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RawSignatureHeader
{
    public uint magic1;
    public uint crc32;
    public uint size_minus_header;
    public uint magic2;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public uint[] void1;
    public uint shifted_sample_rate_id;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public uint[] void2;
    public uint number_samples_plus_divided_sample_rate;
    public uint magic3;
}

/// <summary>
/// CRC32 calculator for signature validation.
/// </summary>
public static class Crc32Algorithm
{
    private static readonly uint[] Table = Enumerable.Range(0, 256)
        .Select(i =>
        {
            uint c = (uint)i;
            for (int j = 0; j < 8; j++)
                c = ((c & 1) != 0) ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            return c;
        }).ToArray();

    public static uint Compute(byte[] data)
    {
        uint crc = 0xFFFFFFFFu;
        foreach (var b in data)
            crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return ~crc;
    }
}