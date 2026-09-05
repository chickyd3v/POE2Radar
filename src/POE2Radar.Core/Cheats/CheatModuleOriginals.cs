using POE2Radar.Core.Game;

namespace POE2Radar.Core.Cheats;

/// <summary>
/// Recovers pre-patch instruction bytes from a unique AOB in an image (typically PathOfExile.exe on disk)
/// when process memory already contains the patched bytes.
/// </summary>
public static class CheatModuleOriginals
{
    public static byte[]? TryExtractUnique(ReadOnlySpan<byte> image, ReadOnlySpan<byte?> pattern, int offset, int count)
    {
        if (count <= 0 || offset < 0) return null;
        var hits = AobScanner.FindPattern(image, pattern);
        if (hits.Count != 1) return null;
        var start = hits[0] + offset;
        if (start < 0 || start + count > image.Length) return null;
        return image.Slice(start, count).ToArray();
    }

    public static byte[]? TryExtractUniqueFromFile(string path, byte?[] pattern, int offset, int count)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        try
        {
            return TryExtractUnique(File.ReadAllBytes(path), pattern, offset, count);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}
