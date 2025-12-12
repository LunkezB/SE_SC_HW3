using FileAnalysisService.Domain;
using Npgsql;

namespace FileAnalysisService.Repositories
{
    public class PostgresWorkRepository(NpgsqlDataSource dataSource) : IWorkRepository
    {
        public async Task InsertAsync(Work work, CancellationToken ct)
        {
            await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
            const string sql = """

                               INSERT INTO works
                               (id, assignment_id, student_id, file_id, submitted_at,
                                word_count, paragraph_count, character_count,
                                is_plagiarism, original_work_id, content_hash, wordcloud_path, status)
                               VALUES (@id, @assignment_id, @student_id, @file_id, @submitted_at,
                                       @word_count, @paragraph_count, @character_count,
                                       @is_plagiarism, @original_work_id, @content_hash, @wordcloud_path, @status);
                               """;

            await using NpgsqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("id", work.Id);
            cmd.Parameters.AddWithValue("assignment_id", work.AssignmentId);
            cmd.Parameters.AddWithValue("student_id", work.StudentId);
            cmd.Parameters.AddWithValue("file_id", work.FileId);
            cmd.Parameters.AddWithValue("submitted_at", work.SubmittedAt);
            cmd.Parameters.AddWithValue("word_count", work.WordCount);
            cmd.Parameters.AddWithValue("paragraph_count", work.ParagraphCount);
            cmd.Parameters.AddWithValue("character_count", work.CharacterCount);
            cmd.Parameters.AddWithValue("is_plagiarism", work.IsPlagiarism);
            cmd.Parameters.AddWithValue("original_work_id", (object?)work.OriginalWorkId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("content_hash", work.ContentHash);
            cmd.Parameters.AddWithValue("wordcloud_path", work.WordCloudPath);
            cmd.Parameters.AddWithValue("status", work.Status.ToString());

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<Work?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
            const string sql = """

                               SELECT id, assignment_id, student_id, file_id, submitted_at,
                                      word_count, paragraph_count, character_count,
                                      is_plagiarism, original_work_id, content_hash, wordcloud_path, status
                               FROM works WHERE id = @id;
                               """;

            await using NpgsqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("id", id);

            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            return Map(reader);
        }

        public async Task<IReadOnlyList<Work>> GetByAssignmentAsync(string assignmentId, CancellationToken ct)
        {
            await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
            const string sql = """

                               SELECT id, assignment_id, student_id, file_id, submitted_at,
                                      word_count, paragraph_count, character_count,
                                      is_plagiarism, original_work_id, content_hash, wordcloud_path, status
                               FROM works
                               WHERE assignment_id = @assignment_id
                               ORDER BY submitted_at;
                               """;

            await using NpgsqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("assignment_id", assignmentId);

            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            List<Work> list = [];
            while (await reader.ReadAsync(ct))
            {
                list.Add(Map(reader));
            }

            return list;
        }

        public async Task<Work?> FindFirstByAssignmentAndHashAsync(string assignmentId, string contentHash, CancellationToken ct)
        {
            await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
            const string sql = """

                               SELECT id, assignment_id, student_id, file_id, submitted_at,
                                      word_count, paragraph_count, character_count,
                                      is_plagiarism, original_work_id, content_hash, wordcloud_path, status
                               FROM works
                               WHERE assignment_id = @assignment_id AND content_hash = @content_hash
                               ORDER BY submitted_at
                               LIMIT 1;
                               """;

            await using NpgsqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("assignment_id", assignmentId);
            cmd.Parameters.AddWithValue("content_hash", contentHash);

            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            return Map(reader);
        }

        private static Work Map(NpgsqlDataReader reader)
        {
            return new Work
            {
                Id = reader.GetGuid(0),
                AssignmentId = reader.GetString(1),
                StudentId = reader.GetString(2),
                FileId = reader.GetGuid(3),
                SubmittedAt = reader.GetFieldValue<DateTimeOffset>(4),
                WordCount = reader.GetInt32(5),
                ParagraphCount = reader.GetInt32(6),
                CharacterCount = reader.GetInt32(7),
                IsPlagiarism = reader.GetBoolean(8),
                OriginalWorkId = reader.IsDBNull(9) ? null : reader.GetGuid(9),
                ContentHash = reader.GetString(10),
                WordCloudPath = reader.GetString(11),
                Status = Enum.Parse<WorkStatus>(reader.GetString(12))
            };
        }
    }
}
