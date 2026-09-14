namespace bagsisbaku.Contracts.Media;

public sealed record UploadedImageResponse(
    Guid? ImageId,
    string Url,
    int Width,
    int Height,
    string Format,
    long Bytes,
    bool? IsPrimary,
    int? SortOrder);
