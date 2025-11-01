
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    points INTEGER DEFAULT 0,
    rank VARCHAR(20) DEFAULT 'Початківець',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE tags (
    tag_id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    creator_id INTEGER REFERENCES users(user_id) ON DELETE SET NULL
);


CREATE TABLE flashcards (
    flashcard_id SERIAL PRIMARY KEY,
    question TEXT NOT NULL,
    answer TEXT NOT NULL,
    creator_id INTEGER REFERENCES users(user_id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE tests (
    test_id SERIAL PRIMARY KEY,
    creator_id INTEGER REFERENCES users(user_id) ON DELETE CASCADE
);


CREATE TABLE test_results (
    test_result_id SERIAL PRIMARY KEY,
    test_id INTEGER REFERENCES tests(test_id) ON DELETE CASCADE,
    user_id INTEGER REFERENCES users(user_id) ON DELETE CASCADE,
    correct_answers_percent NUMERIC(5,2) NOT NULL,
    points INTEGER NOT NULL,
    test_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE question_results (
    question_result_id SERIAL PRIMARY KEY,
    test_result_id INTEGER REFERENCES test_results(test_result_id) ON DELETE CASCADE,
    flashcard_id INTEGER REFERENCES flashcards(flashcard_id) ON DELETE CASCADE,
    user_input TEXT NOT NULL,
    is_correct BOOLEAN NOT NULL
);


CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_flashcards_creator_id ON flashcards(creator_id);
CREATE INDEX idx_test_results_user_id ON test_results(user_id);
CREATE INDEX idx_test_results_test_id ON test_results(test_id);
CREATE INDEX idx_question_results_test_result_id ON question_results(test_result_id);
CREATE INDEX idx_question_results_flashcard_id ON question_results(flashcard_id);
CREATE INDEX idx_tags_creator_id ON tags(creator_id);


CREATE OR REPLACE FUNCTION update_user_rank()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.points BETWEEN 0 AND 100 THEN
        NEW.rank := 'Початківець';
    ELSIF NEW.points BETWEEN 101 AND 500 THEN
        NEW.rank := 'Любитель';
    ELSE
        NEW.rank := 'Професіонал';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_rank
    BEFORE UPDATE OF points ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_user_rank();


SELECT user_id, email, full_name, points, rank, created_at
FROM users
ORDER BY user_id;

SELECT flashcard_id, question, answer, created_at
FROM flashcards
WHERE creator_id = 1
ORDER BY created_at DESC;


SELECT tr.test_result_id, tr.correct_answers_percent, tr.points, tr.test_date
FROM test_results tr
WHERE tr.user_id = 1
ORDER BY tr.test_date DESC;


SELECT qr.user_input, qr.is_correct, f.question, f.answer
FROM question_results qr
JOIN flashcards f ON qr.flashcard_id = f.flashcard_id
WHERE qr.test_result_id = 1;