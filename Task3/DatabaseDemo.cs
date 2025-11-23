namespace AdoNetPostgresDemo
{
    using System;
    using System.IO;
    using Npgsql;

    /// <summary>
    /// Демонстраційний клас для взаємодії з базою даних PostgreSQL за допомогою Ado.Net (Npgsql).
    /// </summary>
    public class DatabaseDemo
    {
        private readonly NpgsqlConnection _conn;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseDemo"/> class.
        /// </summary>
        /// <param name="conn">Активне з'єднання Npgsql, яке буде використовуватися для виконання команд.</param>
        public DatabaseDemo(NpgsqlConnection conn)
        {
            this._conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        /// <summary>
        /// Отримує та виводить список усіх користувачів з таблиці 'users'.
        /// </summary>
        /// <param name="output">Потік для виводу результатів (зазвичай Console.Out).</param>
        public void ShowUsers(TextWriter output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.WriteLine("=== 🧑 Таблиця USERS ===");
            const string query = @"
                SELECT user_id, email, full_name, points, rank, created_at
                FROM users
                ORDER BY user_id;
            ";

            using var cmd = new NpgsqlCommand(query, this._conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                output.WriteLine("❌ Немає користувачів.\n");
                return;
            }

            while (reader.Read())
            {
                output.WriteLine(
                    $"ID: {reader["user_id"]}, " +
                    $"Email: {reader["email"]}, " +
                    $"Ім'я: {reader["full_name"]}, " +
                    $"Очки: {reader["points"]}, " +
                    $"Ранг: {reader["rank"]}, " +
                    $"Створено: {reader["created_at"]}");
            }

            output.WriteLine();
        }

        /// <summary>
        /// Отримує та виводить список усіх флеш-карток з таблиці 'flashcards'.
        /// </summary>
        /// <param name="output">Потік для виводу результатів.</param>
        public void ShowFlashcards(TextWriter output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.WriteLine("=== 🃏 Таблиця FLASHCARDS ===");
            const string query = @"
                SELECT flashcard_id, creator_id, question, answer, created_at
                FROM flashcards
                ORDER BY flashcard_id;
            ";

            using var cmd = new NpgsqlCommand(query, this._conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                output.WriteLine("❌ Немає карток.\n");
                return;
            }

            while (reader.Read())
            {
                output.WriteLine(
                    $"ID: {reader["flashcard_id"]}, " +
                    $"CreatorID: {reader["creator_id"]}, " +
                    $"Питання: {reader["question"]}, " +
                    $"Відповідь: {reader["answer"]}, " +
                    $"Створено: {reader["created_at"]}");
            }

            output.WriteLine();
        }

        /// <summary>
        /// Отримує та виводить список усіх результатів тестів з таблиці 'test_results'.
        /// </summary>
        /// <param name="output">Потік для виводу результатів.</param>
        public void ShowTestResults(TextWriter output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.WriteLine("=== 📊 Таблиця TEST_RESULTS ===");
            const string query = @"
                SELECT test_result_id, test_id, user_id, correct_answers_percent, points, test_date
                FROM test_results
                ORDER BY test_result_id;
            ";

            using var cmd = new NpgsqlCommand(query, this._conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                output.WriteLine("❌ Немає результатів тестів.\n");
                return;
            }

            while (reader.Read())
            {
                output.WriteLine(
                    $"ID: {reader["test_result_id"]}, " +
                    $"TestID: {reader["test_id"]}, " +
                    $"UserID: {reader["user_id"]}, " +
                    $"% правильних: {reader["correct_answers_percent"]}, " +
                    $"Бали: {reader["points"]}, " +
                    $"Дата: {reader["test_date"]}");
            }

            output.WriteLine();
        }

        /// <summary>
        /// Генерує випадкові тестові дані (користувачів, картки, результати) та вставляє їх у БД.
        /// </summary>
        public void GenerateTestData()
        {
            var random = new Random();
            int count = random.Next(30, 51);

            for (int i = 1; i <= count; i++)
            {
                string email = $"user{i}@example.com";
                string fullName = $"Тестовий Користувач {i}";
                int points = random.Next(0, 1000);

                int userId;
                using (var cmd = new NpgsqlCommand(
                           @"INSERT INTO users (email, password_hash, full_name, points) 
                             VALUES (@e, @p, @f, @pts) 
                             RETURNING user_id;",
                           this._conn))
                {
                    cmd.Parameters.AddWithValue("e", email);
                    cmd.Parameters.AddWithValue("p", "testhash");
                    cmd.Parameters.AddWithValue("f", fullName);
                    cmd.Parameters.AddWithValue("pts", points);

                    userId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (var flashCmd = new NpgsqlCommand(
                           @"INSERT INTO flashcards (question, answer, creator_id) 
                             VALUES (@q, @a, @cid);",
                           this._conn))
                {
                    flashCmd.Parameters.AddWithValue("q", $"Питання для {fullName}");
                    flashCmd.Parameters.AddWithValue("a", $"Відповідь для {fullName}");
                    flashCmd.Parameters.AddWithValue("cid", userId);
                    flashCmd.ExecuteNonQuery();
                }

                int testId;
                using (var testCreateCmd = new NpgsqlCommand(
                           @"INSERT INTO tests (creator_id) 
                             VALUES (@cid) 
                             RETURNING test_id;",
                           this._conn))
                {
                    testCreateCmd.Parameters.AddWithValue("cid", userId);
                    testId = Convert.ToInt32(testCreateCmd.ExecuteScalar());
                }

                int totalQuestions = random.Next(5, 11);
                int correctAnswers = random.Next(0, totalQuestions + 1);

                decimal correctPercent = Math.Round(
                    (decimal)correctAnswers * 100m / totalQuestions,
                    2);

                int scorePoints = correctAnswers * 10;

                using (var testResultCmd = new NpgsqlCommand(
                           @"INSERT INTO test_results (test_id, user_id, correct_answers_percent, points) 
                             VALUES (@tid, @uid, @p, @pts);",
                           this._conn))
                {
                    testResultCmd.Parameters.AddWithValue("tid", testId);
                    testResultCmd.Parameters.AddWithValue("uid", userId);
                    testResultCmd.Parameters.AddWithValue("p", correctPercent);
                    testResultCmd.Parameters.AddWithValue("pts", scorePoints);
                    testResultCmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Видаляє всі записи з таблиць, крім першого (MIN ID), для очищення тестових даних.
        /// </summary>
        /// <param name="output">Опційний потік для виводу кількості видалених рядків.</param>
        public void KeepOnlyFirstRecords(TextWriter? output = null)
        {
            using (var cmd = new NpgsqlCommand(
                       @"DELETE FROM users 
                         WHERE user_id <> (SELECT MIN(user_id) FROM users);",
                       this._conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                output?.WriteLine($"🧑 Видалено {rowsDeleted} користувачів, крім першого.");
            }

            using (var cmd = new NpgsqlCommand(
                       @"DELETE FROM flashcards 
                         WHERE flashcard_id <> (SELECT MIN(flashcard_id) FROM flashcards);",
                       this._conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                output?.WriteLine($"🃏 Видалено {rowsDeleted} флеш-карток, крім першої.");
            }

            using (var cmd = new NpgsqlCommand(
                       @"DELETE FROM test_results 
                         WHERE test_result_id <> (SELECT MIN(test_result_id) FROM test_results);",
                       this._conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                output?.WriteLine($"📊 Видалено {rowsDeleted} результатів тестів, крім першого.");
            }
        }
    }
}
