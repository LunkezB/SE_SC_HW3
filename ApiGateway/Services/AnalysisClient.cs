using System.Net.Http.Json;
using System.Text.Json;
using ApiGateway.Dtos;

namespace ApiGateway.Services
{
    public class AnalysisClient(HttpClient client) : IAnalysisClient
    {
        private class CreateWorkRequest
        {
            public string StudentId { get; set; } = null!;
            public string AssignmentId { get; set; } = null!;
            public Guid FileId { get; set; }
        }

        public async Task<SubmitWorkResponse> CreateWorkAsync(string studentId, string assignmentId, Guid fileId, CancellationToken ct)
        {
            CreateWorkRequest payload = new()
            {
                StudentId = studentId,
                AssignmentId = assignmentId,
                FileId = fileId
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("works", payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Analysis service error: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync(ct);
            SubmitWorkResponse? dto = JsonSerializer.Deserialize<SubmitWorkResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return dto ?? throw new InvalidOperationException("Invalid response from analysis service");
        }

        public async Task<IReadOnlyList<WorkReportDto>> GetReportsAsync(string assignmentId, CancellationToken ct)
        {
            HttpResponseMessage response = await client.GetAsync($"works/{assignmentId}/reports", ct);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Analysis service error: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync(ct);
            List<WorkReportDto>? dtos = JsonSerializer.Deserialize<List<WorkReportDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return dtos ?? [];
        }

        public async Task<HttpResponseMessage> GetWordCloudAsync(Guid workId, CancellationToken ct)
        {
            HttpResponseMessage response = await client.GetAsync($"works/{workId}/wordcloud", ct);
            return response;
        }
    }
}
