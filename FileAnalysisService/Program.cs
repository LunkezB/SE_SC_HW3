using FileAnalysisService.Repositories;
using FileAnalysisService.Services;
using Microsoft.OpenApi;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Env: DATABASE_URL, FILE_STORAGE_URL, IMAGES_DIR
string dbConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                            ?? builder.Configuration.GetConnectionString("Default")
                            ?? throw new InvalidOperationException("DATABASE_URL not configured");

string fileStorageUrl = Environment.GetEnvironmentVariable("FILE_STORAGE_URL")
                        ?? throw new InvalidOperationException("FILE_STORAGE_URL not configured");

string imagesDir = Environment.GetEnvironmentVariable("IMAGES_DIR") ?? "/images";

builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    NpgsqlDataSourceBuilder dsb = new(dbConnectionString);
    return dsb.Build();
});

builder.Services.AddScoped<IWorkRepository, PostgresWorkRepository>();
builder.Services.AddScoped<IImageStorage>(_ => new FileSystemImageStorage(imagesDir));
builder.Services.AddScoped<IAnalysisService, AnalysisService>();

builder.Services.AddHttpClient<IFileStorageClient, FileStorageClient>(client =>
{
    client.BaseAddress = new Uri(fileStorageUrl.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<IWordCloudClient, QuickChartWordCloudClient>(client =>
{
    client.BaseAddress = new Uri("https://quickchart.io/");
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FileAnalysisService", Version = "v1" });
});

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileAnalysisService v1");
});

app.MapControllers();

app.Run();