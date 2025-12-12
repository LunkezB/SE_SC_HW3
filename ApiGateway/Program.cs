using ApiGateway.Services;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Env: FILE_STORAGE_URL, ANALYSIS_SERVICE_URL
string fileStorageUrl = Environment.GetEnvironmentVariable("FILE_STORAGE_URL")
                        ?? throw new InvalidOperationException("FILE_STORAGE_URL not configured");

string analysisServiceUrl = Environment.GetEnvironmentVariable("ANALYSIS_SERVICE_URL")
                            ?? throw new InvalidOperationException("ANALYSIS_SERVICE_URL not configured");

builder.Services.AddHttpClient<IFileStoringClient, FileStoringClient>(client =>
{
    client.BaseAddress = new Uri(fileStorageUrl.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<IAnalysisClient, AnalysisClient>(client =>
{
    client.BaseAddress = new Uri(analysisServiceUrl.TrimEnd('/') + "/");
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiGateway", Version = "v1" });
});

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiGateway v1");
});

app.MapControllers();

app.Run();