CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL, 
    full_name VARCHAR(100),
    points INTEGER DEFAULT 0,
    rank VARCHAR(20) DEFAULT 'Початківець',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE flashcards (
    flashcard_id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(user_id) ON DELETE CASCADE,
    question TEXT NOT NULL,
    answer TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE test_results (
    result_id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(user_id) ON DELETE CASCADE,
    correct_answers INTEGER NOT NULL,
    total_questions INTEGER NOT NULL,
    score INTEGER NOT NULL,
    test_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_flashcards_user_id ON flashcards(user_id);
CREATE INDEX idx_test_results_user_id ON test_results(user_id);


CREATE OR REPLACE FUNCTION update_user_rank()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.points BETWEEN 0 AND 100 THEN
        NEW.rank = 'Початківець';
    ELSIF NEW.points BETWEEN 101 AND 500 THEN
        NEW.rank = 'Любитель';
    ELSE
        NEW.rank = 'Професіонал';
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
WHERE user_id = 1
ORDER BY created_at DESC;

SELECT result_id, correct_answers, total_questions, score, test_date
FROM test_results
WHERE user_id = 1
ORDER BY test_date DESC;

