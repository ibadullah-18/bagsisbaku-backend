namespace bagsisbaku.Application.Abstractions.Storage;

public interface IImageStorage
{
    Task<StoredImage> UploadAsync(
        Stream content,
        string fileName,
        ImageFolder folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default);
}
