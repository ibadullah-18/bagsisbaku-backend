namespace bagsisbaku.Application.Abstractions.Storage;

public sealed record StoredImage(
    string Url,
    string PublicId,
    int Width,
    int Height,
    string Format,
    long Bytes);
