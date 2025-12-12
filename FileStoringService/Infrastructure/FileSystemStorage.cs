namespace FileStoringService.Infrastructure
{
    public class FileSystemStorage(string baseDirectory)
    {
        public string BaseDirectory { get; } = baseDirectory;

        public async Task<string> SaveAsync(Guid id, byte[] content, CancellationToken ct = default)
        {
            Directory.CreateDirectory(BaseDirectory);
            string path = Path.Combine(BaseDirectory, $"{id}.txt");

            await using FileStream fs = new(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await fs.WriteAsync(content, ct);

            return path;
        }

        public async Task<byte[]> ReadAsync(string location, CancellationToken ct = default)
        {
            return await File.ReadAllBytesAsync(location, ct);
        }
    }
}