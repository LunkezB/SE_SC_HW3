using FileStoringService.Domain;
using Npgsql;

namespace FileStoringService.Repositories
{
    public class PostgresFileMetadataRepository(NpgsqlDataSource dataSource) : IFileMetadataRepository
    {
        public async Task SaveAsync(StoredFileMetadata file, CancellationToken ct)
        {
            await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
            await using NpgsqlCommand cmd = new(
                "INSERT INTO files (id, name, hash, location) VALUES (@id, @name, @hash, @location)", conn);
            cmd.Parameters.AddWithValue("id", file.Id);
            cmd.Parameters.AddWithValue("name", file.FileName);
            cmd.Parameters.AddWithValue("hash", file.Hash);
            cmd.Parameters.AddWithValue("location", file.Location);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<StoredFileMetadata?> GetAsync(Guid id, CancellationToken ct)
        {
            await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
            await using NpgsqlCommand cmd = new(
                "SELECT id, name, hash, location FROM files WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);

            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            return !await reader.ReadAsync(ct) ? null : new StoredFileMetadata
            {
                Id = reader.GetGuid(0),
                FileName = reader.GetString(1),
                Hash = reader.GetString(2),
                Location = reader.GetString(3)
            };
        }
    }
}