using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Dtos
{
    public class SubmitWorkRequest
    {
        [FromForm(Name = "studentId")]
        public string StudentId { get; set; } = string.Empty;

        [FromForm(Name = "assignmentId")]
        public string AssignmentId { get; set; } = string.Empty;

        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = null!;
    }
}