namespace FileAnalysisService.Dtos
{
    public class CreateWorkRequestDto
    {
        public string StudentId { get; set; } = null!;
        public string AssignmentId { get; set; } = null!;
        public Guid FileId { get; set; }
    }
}