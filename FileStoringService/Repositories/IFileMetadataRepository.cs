using FileStoringService.Domain;

namespace FileStoringService.Repositories
{
    public interface IFileMetadataRepository
    {
        Task SaveAsync(StoredFileMetadata file, CancellationToken ct);
        Task<StoredFileMetadata?> GetAsync(Guid id, CancellationToken ct);
    }
}