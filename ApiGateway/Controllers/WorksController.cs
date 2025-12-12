using ApiGateway.Dtos;
using ApiGateway.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/works")]
    public class WorksController(
        IFileStoringClient fileClient,
        IAnalysisClient analysisClient,
        ILogger<WorksController> logger)
        : ControllerBase
    {
        /// <summary>
        /// Отправка работы на проверку (студент).
        /// </summary>
        [HttpPost("submit")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(SubmitWorkResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Submit(
            [FromForm] SubmitWorkRequest request,
            CancellationToken ct)
        {
            string studentId = request.StudentId;
            string assignmentId = request.AssignmentId;
            IFormFile file = request.File;

            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(assignmentId))
            {
                return BadRequest(new { error = "studentId and assignmentId are required" });
            }

            if (file == null)
            {
                return BadRequest(new { error = "file is required" });
            }

            if (Path.GetExtension(file.FileName) != ".txt")
            {
                return BadRequest(new { error = "Only .txt files are allowed" });
            }

            try
            {
                Guid fileId = await fileClient.UploadFileAsync(file, ct);

                SubmitWorkResponse result =
                    await analysisClient.CreateWorkAsync(studentId, assignmentId, fileId, ct);

                result.WordCloudUrl = Url.ActionLink(
                    nameof(GetWordCloud),
                    values: new { workId = result.WorkId }
                )!;

                return CreatedAtAction(nameof(GetReports), new { assignmentId }, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while submitting work");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { error = "Upstream service error" });
            }
        }

        /// <summary>
        /// Получить отчёты по заданию (преподаватель).
        /// </summary>
        [HttpGet("{assignmentId}/reports")]
        [ProducesResponseType(typeof(IEnumerable<WorkReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetReports(string assignmentId, CancellationToken ct)
        {
            try
            {
                IReadOnlyList<WorkReportDto> reports = await analysisClient.GetReportsAsync(assignmentId, ct);

                foreach (var r in reports)
                {
                    r.WordCloudUrl = Url.ActionLink(nameof(GetWordCloud), values: new { workId = r.WorkId })!;
                }

                return Ok(reports);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching reports");
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Upstream service error" });
            }
        }

        /// <summary>
        /// Получить облако слов (PNG) для конкретной работы по её id.
        /// </summary>
        [HttpGet("{workId:guid}/wordcloud")]
        [Produces("image/png")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetWordCloud(Guid workId, CancellationToken ct)
        {
            try
            {
                HttpResponseMessage response = await analysisClient.GetWordCloudAsync(workId, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode);
                }

                byte[] bytes = await response.Content.ReadAsByteArrayAsync(ct);
                return File(bytes, "image/png");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching wordcloud");
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Upstream service error" });
            }
        }
    }
}
