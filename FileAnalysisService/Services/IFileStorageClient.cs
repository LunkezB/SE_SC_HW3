namespace FileAnalysisService.Services
{
    public interface IFileStorageClient
    {
        Task<string> GetFileTextAsync(Guid fileId, CancellationToken ct);
    }
}