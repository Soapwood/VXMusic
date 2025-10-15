using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace VXMusic.Recognition.Shazam.Keyless;

/// <summary>
/// The Shazam signature (decoded or to be encoded).
/// </summary>
public class DecodedMessage
{
    private const string DataUriPrefix = "data:audio/vnd.shazam.sig;base64,";
    private const int HeaderSize = 48;
    private const uint HeaderMagic1 = 0xCAFE2580;
    private const uint HeaderMagic2 = 0x94119C00;
    private const uint HeaderMagic3 = ((15u << 19) + 0x40000);

    // sample‑rate ↔ id mapping:
    private static readonly IReadOnlyDictionary<uint, int> ShiftedSampleRateFromId = new Dictionary<uint, int>
    {
        {1u << 27,  8000},
        {2u << 27, 11025},
        {3u << 27, 16000},
        {4u << 27, 32000},
        {5u << 27, 44100},
        {6u << 27, 48000},
    };
    private static readonly IReadOnlyDictionary<int, uint> ShiftedSampleRateToId =
        ShiftedSampleRateFromId.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public int SampleRateHz { get; set; }
    public int NumberSamples { get; set; }
    public Dictionary<FrequencyBand, List<FrequencyPeak>> FrequencyBandToSoundPeaks { get; set; }
        = new Dictionary<FrequencyBand, List<FrequencyPeak>>();

    public static DecodedMessage DecodeFromBinary(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

        // Read header
        var header = reader.ReadBytes(HeaderSize);
        var hdrHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
        RawSignatureHeader rawHeader;
        try
        {
            rawHeader = Marshal.PtrToStructure<RawSignatureHeader>(hdrHandle.AddrOfPinnedObject());
        }
        finally
        {
            hdrHandle.Free();
        }

        if (rawHeader.magic1 != HeaderMagic1 || rawHeader.magic2 != HeaderMagic2)
            throw new InvalidDataException("Invalid header magic.");

        if (rawHeader.size_minus_header != data.Length - HeaderSize)
            throw new InvalidDataException("Invalid size in header.");

        // CRC check
        ms.Position = 8;
        var rest = reader.ReadBytes(data.Length - 8);
        var crc = Crc32Algorithm.Compute(rest);
        if (crc != rawHeader.crc32)
            throw new InvalidDataException("CRC mismatch.");

        var msg = new DecodedMessage
        {
            SampleRateHz = ShiftedSampleRateFromId[rawHeader.shifted_sample_rate_id],
            NumberSamples = (int)(rawHeader.number_samples_plus_divided_sample_rate - rawHeader.shifted_sample_rate_id switch
            {
                uint id when ShiftedSampleRateFromId.ContainsKey(id) => ShiftedSampleRateFromId[id] * 0.24,
                _ => 0
            })
        };

        // Skip the initial TLV
        ms.Position = HeaderSize + 8;

        // Read TLVs
        while (ms.Position < ms.Length)
        {
            var bandId = reader.ReadUInt32();
            var size = reader.ReadInt32();
            var payload = reader.ReadBytes(size);
            var padding = (4 - (size % 4)) % 4;
            ms.Position += padding;

            var band = (FrequencyBand)(bandId - 0x60030040);
            var peaks = new List<FrequencyPeak>();
            using var pms = new MemoryStream(payload);
            using var pr = new BinaryReader(pms);

            int fftPass = 0;
            while (pms.Position < pms.Length)
            {
                byte offset = pr.ReadByte();
                if (offset == 0xFF)
                {
                    fftPass = pr.ReadInt32();
                    continue;
                }
                fftPass += offset;
                int magnitude = pr.ReadUInt16();
                int freqBin = pr.ReadUInt16();
                peaks.Add(new FrequencyPeak(fftPass, magnitude, freqBin, msg.SampleRateHz));
            }

            msg.FrequencyBandToSoundPeaks[band] = peaks;
        }

        return msg;
    }

    public static DecodedMessage DecodeFromUri(string uri)
    {
        if (!uri.StartsWith(DataUriPrefix))
            throw new ArgumentException("Not a valid Shazam data URI.");
        var bin = Convert.FromBase64String(uri.Substring(DataUriPrefix.Length));
        return DecodeFromBinary(bin);
    }

    public byte[] EncodeToBinary()
    {
        const int HeaderSize = 48;
        const uint HeaderMagic1 = 0xCAFE2580;
        const uint HeaderMagic2 = 0x94119C00;
        const uint HeaderMagic3 = (15u << 19) + 0x40000;

        // 1) Build the TLV "content" payload
        byte[] contents;
        using (var contentMs = new MemoryStream())
        using (var contentWriter = new BinaryWriter(contentMs, Encoding.UTF8, leaveOpen: true))
        {
            foreach (var kv in FrequencyBandToSoundPeaks.OrderBy(k => k.Key))
            {
                using var peaksMs = new MemoryStream();
                using var pw = new BinaryWriter(peaksMs, Encoding.UTF8, leaveOpen: true);
                int fftPass = 0;

                foreach (var peak in kv.Value)
                {
                    if (peak.FftPassNumber - fftPass >= 255)
                    {
                        pw.Write((byte)0xFF);
                        pw.Write(peak.FftPassNumber);
                        fftPass = peak.FftPassNumber;
                    }
                    pw.Write((byte)(peak.FftPassNumber - fftPass));
                    pw.Write((ushort)peak.PeakMagnitude);
                    pw.Write((ushort)peak.CorrectedPeakFrequencyBin);
                    fftPass = peak.FftPassNumber;
                }

                var payload = peaksMs.ToArray();
                contentWriter.Write((uint)(0x60030040 + (int)kv.Key));
                contentWriter.Write(payload.Length);
                contentWriter.Write(payload);
                int pad = (4 - (payload.Length % 4)) % 4;
                for (int i = 0; i < pad; i++) contentWriter.Write((byte)0);
            }

            contents = contentMs.ToArray();
        }

        uint sizeMinusHeader = (uint)(contents.Length + 8);

        // 2) Prepare the header struct with crc32 = 0
        var header = new RawSignatureHeader
        {
            magic1 = HeaderMagic1,
            crc32 = 0, // placeholder
            size_minus_header = sizeMinusHeader,
            magic2 = HeaderMagic2,
            void1 = new uint[3],          // ensure these are zeroed
            shifted_sample_rate_id = ShiftedSampleRateToId[SampleRateHz],
            void2 = new uint[2],          // ensure these are zeroed
            number_samples_plus_divided_sample_rate = (uint)(NumberSamples + SampleRateHz * 0.24),
            magic3 = HeaderMagic3
        };

        // Marshal header (with crc32=0) into bytes
        int hdrSize = Marshal.SizeOf<RawSignatureHeader>();
        var headerBytes = new byte[hdrSize];
        var ptr = Marshal.AllocHGlobal(hdrSize);
        try
        {
            Marshal.StructureToPtr(header, ptr, false);
            Marshal.Copy(ptr, headerBytes, 0, hdrSize);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        // 3) Write header + TLV + content
        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
        {
            bw.Write(headerBytes);                // real header (crc32 still zero)
            bw.Write(0x40000000);                // TLV magic
            bw.Write(sizeMinusHeader);           // TLV length
            bw.Write(contents);                  // the fingerprint data
        }

        // 4) Compute CRC‑32 over everything after the first 8 bytes
        ms.Position = 8;
        var rest = new byte[ms.Length - 8];
        ms.Read(rest, 0, rest.Length);
        uint crc = Crc32Algorithm.Compute(rest);

        // 5) Patch only the CRC field (bytes 4–7) in the existing header
        var crcBytes = BitConverter.GetBytes(crc);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(crcBytes);
        ms.Position = 4;
        ms.Write(crcBytes, 0, 4);

        return ms.ToArray();
    }

    public string EncodeToUri() =>
        DataUriPrefix + Convert.ToBase64String(EncodeToBinary());

    public object ToJson() =>
        new
        {
            sample_rate_hz = SampleRateHz,
            number_samples = NumberSamples,
            _seconds = (double)NumberSamples / SampleRateHz,
            frequency_band_to_peaks = FrequencyBandToSoundPeaks.ToDictionary(
                kv => kv.Key.ToString(),
                kv => kv.Value.Select(fp => new
                {
                    fft_pass_number = fp.FftPassNumber,
                    peak_magnitude = fp.PeakMagnitude,
                    corrected_peak_frequency_bin = fp.CorrectedPeakFrequencyBin,
                    _frequency_hz = fp.GetFrequencyHz(),
                    _amplitude_pcm = fp.GetAmplitudePcm(),
                    _seconds = fp.GetSeconds()
                }).ToArray()
            )
        };

    public override string ToString() =>
        JsonConvert.SerializeObject(ToJson(), Formatting.Indented);
}