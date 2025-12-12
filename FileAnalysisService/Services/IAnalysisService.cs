using FileAnalysisService.Domain;
using FileAnalysisService.Dtos;

namespace FileAnalysisService.Services
{
    public interface IAnalysisService
    {
        Task<Work> AnalyzeAsync(CreateWorkRequestDto request, CancellationToken ct);
    }
}