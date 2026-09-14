using System.Text;
using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Media;

public static class ImageFileValidator
{
    private const long StandardMaximumBytes =
        10L * 1024 * 1024;

    private const long HeicMaximumBytes =
        25L * 1024 * 1024;

    private static readonly HashSet<string>
        AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".heic",
            ".heif"
        };

    public static async Task<Error?> ValidateAsync(
        ImageUploadCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Content);

        if (command.Length <= 0)
        {
            return MediaErrors.EmptyFile;
        }

        var extension = Path.GetExtension(
            command.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            return MediaErrors.UnsupportedFormat;
        }

        var isHeic =
            extension.Equals(
                ".heic",
                StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(
                ".heif",
                StringComparison.OrdinalIgnoreCase);

        var maximumBytes = isHeic
            ? HeicMaximumBytes
            : StandardMaximumBytes;

        if (command.Length > maximumBytes)
        {
            return isHeic
                ? MediaErrors.HeicFileTooLarge
                : MediaErrors.StandardFileTooLarge;
        }

        if (!command.Content.CanRead ||
            !command.Content.CanSeek)
        {
            return MediaErrors.InvalidStream;
        }

        command.Content.Position = 0;

        var header = new byte[12];

        var bytesRead = await command.Content.ReadAsync(
            header.AsMemory(0, header.Length),
            cancellationToken);

        command.Content.Position = 0;

        var hasValidSignature = extension
            .ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" =>
                IsJpeg(header, bytesRead),

            ".png" =>
                IsPng(header, bytesRead),

            ".webp" =>
                IsWebp(header, bytesRead),

            ".heic" or ".heif" =>
                IsHeic(header, bytesRead),

            _ => false
        };

        return hasValidSignature
            ? null
            : MediaErrors.InvalidSignature;
    }

    private static bool IsJpeg(
        byte[] header,
        int bytesRead)
    {
        return bytesRead >= 3 &&
               header[0] == 0xFF &&
               header[1] == 0xD8 &&
               header[2] == 0xFF;
    }

    private static bool IsPng(
        byte[] header,
        int bytesRead)
    {
        return bytesRead >= 8 &&
               header[0] == 0x89 &&
               header[1] == 0x50 &&
               header[2] == 0x4E &&
               header[3] == 0x47 &&
               header[4] == 0x0D &&
               header[5] == 0x0A &&
               header[6] == 0x1A &&
               header[7] == 0x0A;
    }

    private static bool IsWebp(
        byte[] header,
        int bytesRead)
    {
        if (bytesRead < 12)
        {
            return false;
        }

        var riff = Encoding.ASCII.GetString(
            header,
            0,
            4);

        var webp = Encoding.ASCII.GetString(
            header,
            8,
            4);

        return riff == "RIFF" && webp == "WEBP";
    }

    private static bool IsHeic(
        byte[] header,
        int bytesRead)
    {
        if (bytesRead < 12)
        {
            return false;
        }

        var fileType = Encoding.ASCII.GetString(
            header,
            4,
            4);

        var brand = Encoding.ASCII.GetString(
            header,
            8,
            4);

        return fileType == "ftyp" &&
               brand is
                   "heic" or
                   "heix" or
                   "hevc" or
                   "hevx" or
                   "heif" or
                   "mif1" or
                   "msf1";
    }
}
