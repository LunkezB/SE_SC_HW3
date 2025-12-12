using FileAnalysisService.Domain;

namespace FileAnalysisService.Repositories
{
    public interface IWorkRepository
    {
        Task InsertAsync(Work work, CancellationToken ct);
        Task<Work?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<Work>> GetByAssignmentAsync(string assignmentId, CancellationToken ct);
        Task<Work?> FindFirstByAssignmentAndHashAsync(string assignmentId, string contentHash, CancellationToken ct);
    }
}