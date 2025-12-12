using FileStoringService.Models;
using FileStoringService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileStoringService.Controllers
{
    [ApiController]
    [Route("files")]
    public class FilesController(IFileStorageService service) : ControllerBase
    {
        /// <summary>Сохранить текстовый файл.</summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Store(
            [FromForm] StoreFileRequest request,
            CancellationToken ct)
        {
            IFormFile file = request.File;

            if (file == null)
            {
                return BadRequest(new { error = "File is required" });
            }

            if (Path.GetExtension(file.FileName) != ".txt")
            {
                return BadRequest(new { error = "Only .txt files are allowed" });
            }

            await using Stream stream = file.OpenReadStream();
            Guid id = await service.StoreAsync(file.FileName, stream, ct);

            return Ok(new { id });
        }

        /// <summary>Получить содержимое файла по id.</summary>
        [HttpGet("{id:guid}")]
        [Produces("text/plain")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            try
            {
                byte[] content = await service.GetContentAsync(id, ct);
                return File(content, "text/plain");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}