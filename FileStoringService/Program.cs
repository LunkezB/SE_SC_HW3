using FileStoringService.Infrastructure;
using FileStoringService.Repositories;
using FileStoringService.Services;
using Microsoft.OpenApi;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Env: DATABASE_URL, FILES_DIR
string dbConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                            ?? builder.Configuration.GetConnectionString("Default")
                            ?? throw new InvalidOperationException("DATABASE_URL not configured");

string filesDir = Environment.GetEnvironmentVariable("FILES_DIR") ?? "/files";

builder.Services.AddSingleton(new FileSystemStorage(filesDir));

builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
{
    NpgsqlDataSourceBuilder dataSourceBuilder = new(dbConnectionString);
    return dataSourceBuilder.Build();
});

builder.Services.AddScoped<IFileMetadataRepository, PostgresFileMetadataRepository>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FileStoringService", Version = "v1" });
});

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileStoringService v1");
});

app.MapControllers();

app.Run();