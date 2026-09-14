namespace bagsisbaku.Application.Media;

public sealed record ImageUploadCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);
