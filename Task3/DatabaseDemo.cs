using System;
using System.IO;
using Npgsql;

namespace AdoNetPostgresDemo
{
    public class DatabaseDemo
    {
        private readonly NpgsqlConnection _conn;

        public DatabaseDemo(NpgsqlConnection conn)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        // USERS

        public void ShowUsers(TextWriter output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteLine("=== 🧑 Таблиця USERS ===");
            const string query = @"
                SELECT user_id, email, full_name, points, rank, created_at
                FROM users
                ORDER BY user_id;
            ";

            using var cmd = new NpgsqlCommand(query, _conn);
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

        // FLASHCARDS

        public void ShowFlashcards(TextWriter output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteLine("=== 🃏 Таблиця FLASHCARDS ===");
            // У схемі БД є creator_id, а не user_id
            const string query = @"
                SELECT flashcard_id, creator_id, question, answer, created_at
                FROM flashcards
                ORDER BY flashcard_id;
            ";

            using var cmd = new NpgsqlCommand(query, _conn);
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

        // TEST_RESULTS

        public void ShowTestResults(TextWriter output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteLine("=== 📊 Таблиця TEST_RESULTS ===");
            const string query = @"
                SELECT test_result_id, test_id, user_id, correct_answers_percent, points, test_date
                FROM test_results
                ORDER BY test_result_id;
            ";

            using var cmd = new NpgsqlCommand(query, _conn);
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

        // TEST DATA GENERATION

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
                           _conn))
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
                           _conn))
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
                           _conn))
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
                           _conn))
                {
                    testResultCmd.Parameters.AddWithValue("tid", testId);
                    testResultCmd.Parameters.AddWithValue("uid", userId);
                    testResultCmd.Parameters.AddWithValue("p", correctPercent);
                    testResultCmd.Parameters.AddWithValue("pts", scorePoints);
                    testResultCmd.ExecuteNonQuery();
                }
            }
        }

        // CLEANUP
        
        public void KeepOnlyFirstRecords(TextWriter? output = null)
        {
            // USERS
            using (var cmd = new NpgsqlCommand(
                       @"DELETE FROM users 
                         WHERE user_id <> (SELECT MIN(user_id) FROM users);",
                       _conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                output?.WriteLine($"🧑 Видалено {rowsDeleted} користувачів, крім першого.");
            }

            // FLASHCARDS
            using (var cmd = new NpgsqlCommand(
                       @"DELETE FROM flashcards 
                         WHERE flashcard_id <> (SELECT MIN(flashcard_id) FROM flashcards);",
                       _conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                output?.WriteLine($"🃏 Видалено {rowsDeleted} флеш-карток, крім першої.");
            }

            // TEST_RESULTS
            using (var cmd = new NpgsqlCommand(
                       @"DELETE FROM test_results 
                         WHERE test_result_id <> (SELECT MIN(test_result_id) FROM test_results);",
                       _conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                output?.WriteLine($"📊 Видалено {rowsDeleted} результатів тестів, крім першого.");
            }
        }
    }
}
