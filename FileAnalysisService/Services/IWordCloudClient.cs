namespace FileAnalysisService.Services
{
    public interface IWordCloudClient
    {
        Task<byte[]> GenerateAsync(string text, CancellationToken ct);
    }
}