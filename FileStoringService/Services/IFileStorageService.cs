namespace FileStoringService.Services
{
    public interface IFileStorageService
    {
        Task<Guid> StoreAsync(string fileName, Stream stream, CancellationToken ct);
        Task<byte[]> GetContentAsync(Guid id, CancellationToken ct);
    }
}