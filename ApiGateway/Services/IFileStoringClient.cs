using Microsoft.AspNetCore.Http;

namespace ApiGateway.Services
{
    public interface IFileStoringClient
    {
        Task<Guid> UploadFileAsync(IFormFile file, CancellationToken ct);
    }
}