using FileAnalysisService.Domain;
using FileAnalysisService.Dtos;
using FileAnalysisService.Repositories;
using FileAnalysisService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileAnalysisService.Controllers
{
    [ApiController]
    [Route("works")]
    public class WorksController(
        IAnalysisService analysisService,
        IWorkRepository repo,
        IImageStorage imageStorage,
        ILogger<WorksController> logger)
        : ControllerBase
    {
        /// <summary>Создать отчёт по работе (анализ).</summary>
        [HttpPost]
        [ProducesResponseType(typeof(WorkReportDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateWorkRequestDto request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.StudentId) || string.IsNullOrWhiteSpace(request.AssignmentId))
            {
                return BadRequest(new { error = "StudentId and AssignmentId are required" });
            }

            try
            {
                Work work = await analysisService.AnalyzeAsync(request, ct);

                WorkReportDto dto = MapToReport(work, Request);

                return CreatedAtAction(nameof(GetWordCloud), new { workId = work.Id }, dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while analyzing work");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal error" });
            }
        }

        /// <summary>Получить отчёты по заданию.</summary>
        [HttpGet("{assignmentId}/reports")]
        [ProducesResponseType(typeof(IEnumerable<WorkReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReports(string assignmentId, CancellationToken ct)
        {
            IReadOnlyList<Work> works = await repo.GetByAssignmentAsync(assignmentId, ct);
            List<WorkReportDto> list = works.Select(w => MapToReport(w, Request)).ToList();
            return Ok(list);
        }

        /// <summary>Получить облако слов по id отчёта.</summary>
        [HttpGet("{workId:guid}/wordcloud")]
        [Produces("image/png")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWordCloud(Guid workId, CancellationToken ct)
        {
            Work? work = await repo.GetByIdAsync(workId, ct);
            if (work == null)
            {
                return NotFound();
            }

            Stream stream = await imageStorage.OpenReadAsync(work.WordCloudPath, ct);
            return File(stream, "image/png");
        }

        private static WorkReportDto MapToReport(Work work, HttpRequest request)
        {
            string baseUrl = $"{request.Scheme}://{request.Host}";
            string wordCloudUrl = $"{baseUrl}/works/{work.Id}/wordcloud";

            return new WorkReportDto
            {
                WorkId = work.Id,
                StudentId = work.StudentId,
                AssignmentId = work.AssignmentId,
                SubmittedAt = work.SubmittedAt,
                WordCount = work.WordCount,
                ParagraphCount = work.ParagraphCount,
                CharacterCount = work.CharacterCount,
                IsPlagiarism = work.IsPlagiarism,
                OriginalWorkId = work.OriginalWorkId,
                Status = work.Status.ToString(),
                WordCloudUrl = wordCloudUrl
            };
        }
    }
}
