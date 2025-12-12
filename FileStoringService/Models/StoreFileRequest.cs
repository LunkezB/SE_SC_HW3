using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileStoringService.Models
{
    public class StoreFileRequest
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = null!;
    }
}