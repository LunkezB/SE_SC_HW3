using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ApiGateway.Services
{
    public class FileStoringClient(HttpClient client) : IFileStoringClient
    {
        private record UploadResponse(Guid Id);

        public async Task<Guid> UploadFileAsync(IFormFile file, CancellationToken ct)
        {
            using MultipartFormDataContent content = new();
            await using Stream stream = file.OpenReadStream();
            StreamContent streamContent = new(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            content.Add(streamContent, "file", file.FileName);

            HttpResponseMessage response = await client.PostAsync("files", content, ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"File storing service error: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync(ct);
            UploadResponse? uploadResp = JsonSerializer.Deserialize<UploadResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return uploadResp?.Id ?? throw new InvalidOperationException("Invalid response from file storing service");
        }
    }
}