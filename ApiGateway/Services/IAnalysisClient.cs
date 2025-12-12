using ApiGateway.Dtos;

namespace ApiGateway.Services
{
    public interface IAnalysisClient
    {
        Task<SubmitWorkResponse> CreateWorkAsync(string studentId, string assignmentId, Guid fileId, CancellationToken ct);
        Task<IReadOnlyList<WorkReportDto>> GetReportsAsync(string assignmentId, CancellationToken ct);
        Task<HttpResponseMessage> GetWordCloudAsync(Guid workId, CancellationToken ct);
    }
}