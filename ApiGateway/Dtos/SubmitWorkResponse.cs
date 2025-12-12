namespace ApiGateway.Dtos
{
    public class SubmitWorkResponse
    {
        public Guid WorkId { get; set; }
        public string StudentId { get; set; } = null!;
        public string AssignmentId { get; set; } = null!;
        public DateTimeOffset SubmittedAt { get; set; }

        public int WordCount { get; set; }
        public int ParagraphCount { get; set; }
        public int CharacterCount { get; set; }

        public bool IsPlagiarism { get; set; }
        public Guid? OriginalWorkId { get; set; }

        public string Status { get; set; } = null!;
        public string WordCloudUrl { get; set; } = null!;
    }
}