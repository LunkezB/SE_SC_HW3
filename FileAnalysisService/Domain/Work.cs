namespace FileAnalysisService.Domain
{
    public class Work
    {
        public Guid Id { get; set; }
        public string AssignmentId { get; set; } = null!;
        public string StudentId { get; set; } = null!;
        public Guid FileId { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }

        public int WordCount { get; set; }
        public int ParagraphCount { get; set; }
        public int CharacterCount { get; set; }

        public bool IsPlagiarism { get; set; }
        public Guid? OriginalWorkId { get; set; }
        public string ContentHash { get; set; } = null!;

        public string WordCloudPath { get; set; } = null!;
        public WorkStatus Status { get; set; }
    }
}