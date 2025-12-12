namespace FileAnalysisService.Services
{
    public class FileStorageClient(HttpClient client) : IFileStorageClient
    {
        public async Task<string> GetFileTextAsync(Guid fileId, CancellationToken ct)
        {
            HttpResponseMessage response = await client.GetAsync($"files/{fileId}", ct);
            return !response.IsSuccessStatusCode ? throw new InvalidOperationException($"Failed to fetch file text: {response.StatusCode}") : await response.Content.ReadAsStringAsync(ct);
        }
    }
}