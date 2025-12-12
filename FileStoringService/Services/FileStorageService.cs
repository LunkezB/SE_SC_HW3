using System.Security.Cryptography;
using FileStoringService.Domain;
using FileStoringService.Infrastructure;
using FileStoringService.Repositories;

namespace FileStoringService.Services
{
    public class FileStorageService(IFileMetadataRepository metadataRepo, FileSystemStorage storage)
        : IFileStorageService
    {
        public async Task<Guid> StoreAsync(string fileName, Stream stream, CancellationToken ct)
        {
            using MemoryStream ms = new();
            await stream.CopyToAsync(ms, ct);
            byte[] bytes = ms.ToArray();
            byte[] hashBytes = SHA256.HashData(bytes);
            string hashStr = Convert.ToHexString(hashBytes).ToLowerInvariant();

            Guid id = Guid.NewGuid();
            string path = await storage.SaveAsync(id, bytes, ct);

            StoredFileMetadata meta = new()
            {
                Id = id,
                FileName = fileName,
                Hash = hashStr,
                Location = path
            };

            await metadataRepo.SaveAsync(meta, ct);
            return id;
        }

        public async Task<byte[]> GetContentAsync(Guid id, CancellationToken ct)
        {
            StoredFileMetadata meta = await metadataRepo.GetAsync(id, ct)
                                      ?? throw new FileNotFoundException("File not found", id.ToString());

            return await storage.ReadAsync(meta.Location, ct);
        }
    }
}