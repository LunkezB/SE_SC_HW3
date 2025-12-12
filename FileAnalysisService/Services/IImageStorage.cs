namespace FileAnalysisService.Services
{
    public interface IImageStorage
    {
        Task<string> SaveImageAsync(Guid workId, byte[] data, CancellationToken ct);
        Task<Stream> OpenReadAsync(string path, CancellationToken ct);
    }
}