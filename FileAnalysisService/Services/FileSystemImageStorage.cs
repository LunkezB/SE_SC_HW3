namespace FileAnalysisService.Services
{
    public class FileSystemImageStorage(string baseDir) : IImageStorage
    {
        public async Task<string> SaveImageAsync(Guid workId, byte[] data, CancellationToken ct)
        {
            Directory.CreateDirectory(baseDir);
            string path = Path.Combine(baseDir, $"{workId}.png");
            await File.WriteAllBytesAsync(path, data, ct);
            return path;
        }

        public Task<Stream> OpenReadAsync(string path, CancellationToken ct)
        {
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult(stream);
        }
    }
}