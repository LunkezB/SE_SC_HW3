namespace FileStoringService.Domain
{
    public class StoredFileMetadata
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = null!;
        public string Hash { get; set; } = null!;
        public string Location { get; set; } = null!;
    }
}