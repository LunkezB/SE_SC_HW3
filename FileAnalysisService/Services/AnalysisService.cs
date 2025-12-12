using System.Security.Cryptography;
using System.Text;
using FileAnalysisService.Domain;
using FileAnalysisService.Dtos;
using FileAnalysisService.Repositories;

namespace FileAnalysisService.Services
{
    public class AnalysisService(
        IWorkRepository repo,
        IFileStorageClient fileStorage,
        IWordCloudClient wordCloudClient,
        IImageStorage imageStorage)
        : IAnalysisService
    {
        public async Task<Work> AnalyzeAsync(CreateWorkRequestDto request, CancellationToken ct)
        {
            string text = await fileStorage.GetFileTextAsync(request.FileId, ct);
            
            (int wordCount, int paragraphCount, int charCount) = CountStats(text);
            
            string contentHash = ComputeHash(text);
            
            Work? existing = await repo.FindFirstByAssignmentAndHashAsync(request.AssignmentId, contentHash, ct);

            DateTimeOffset now = DateTimeOffset.UtcNow;
            Work work = new()
            {
                Id = Guid.NewGuid(),
                AssignmentId = request.AssignmentId,
                StudentId = request.StudentId,
                FileId = request.FileId,
                SubmittedAt = now,
                WordCount = wordCount,
                ParagraphCount = paragraphCount,
                CharacterCount = charCount,
                ContentHash = contentHash,
                IsPlagiarism = false,
                OriginalWorkId = null,
                Status = WorkStatus.Completed
            };

            if (existing != null && existing.StudentId != request.StudentId && existing.SubmittedAt < now)
            {
                work.IsPlagiarism = true;
                work.OriginalWorkId = existing.Id;
            }
            
            byte[] imgBytes = await wordCloudClient.GenerateAsync(text, ct);
            string path = await imageStorage.SaveImageAsync(work.Id, imgBytes, ct);
            work.WordCloudPath = path;
            
            await repo.InsertAsync(work, ct);

            return work;
        }

        private static (int wordCount, int paragraphCount, int charCount) CountStats(string text)
        {
            string[] words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            int wordCount = words.Length;

            string normalized = text.Replace("\r\n", "\n");
            string[] paragraphs = normalized.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
            int paragraphCount = paragraphs.Length;

            int charCount = text.Length;

            return (wordCount, paragraphCount, charCount);
        }

        private static string ComputeHash(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hashBytes = SHA256.HashData(bytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
