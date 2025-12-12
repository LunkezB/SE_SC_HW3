using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FileAnalysisService.Services
{
    public class QuickChartWordCloudClient(HttpClient client) : IWordCloudClient
    {
        private class WordCloudPayload
        {
            [JsonPropertyName("format")]
            public string Format { get; set; } = "png";

            [JsonPropertyName("width")]
            public int Width { get; set; } = 500;

            [JsonPropertyName("height")]
            public int Height { get; set; } = 500;

            [JsonPropertyName("text")]
            public string Text { get; set; } = null!;

            [JsonPropertyName("fontScale")]
            public int FontScale { get; set; } = 15;
        }

        public async Task<byte[]> GenerateAsync(string text, CancellationToken ct)
        {
            WordCloudPayload payload = new() { Text = text };

            HttpResponseMessage response = await client.PostAsJsonAsync("wordcloud", payload, ct);
            
            return !response.IsSuccessStatusCode ? throw new InvalidOperationException($"Word cloud API error: {response.StatusCode}") : await response.Content.ReadAsByteArrayAsync(ct);
        }
    }
}