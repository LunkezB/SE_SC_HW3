CREATE TABLE IF NOT EXISTS works
(
    id
    UUID
    PRIMARY
    KEY,
    assignment_id
    TEXT
    NOT
    NULL,
    student_id
    TEXT
    NOT
    NULL,
    file_id
    UUID
    NOT
    NULL,
    submitted_at
    TIMESTAMPTZ
    NOT
    NULL,

    word_count
    INT
    NOT
    NULL,
    paragraph_count
    INT
    NOT
    NULL,
    character_count
    INT
    NOT
    NULL,

    is_plagiarism
    BOOLEAN
    NOT
    NULL,
    original_work_id
    UUID
    NULL,
    content_hash
    TEXT
    NOT
    NULL,

    wordcloud_path
    TEXT
    NOT
    NULL,
    status
    TEXT
    NOT
    NULL
);

CREATE INDEX IF NOT EXISTS idx_works_assignment ON works (assignment_id);
CREATE INDEX IF NOT EXISTS idx_works_assignment_hash ON works (assignment_id, content_hash);
