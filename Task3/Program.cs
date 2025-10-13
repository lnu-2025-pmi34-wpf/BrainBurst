using System;
using System.Text;
using Npgsql;
using DotNetEnv;

namespace AdoNetPostgresDemo
{
    internal class Program
    {
        static string connectionString;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Env.Load();
            connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            Console.WriteLine("Підключення успішне!\n");
            //KeepOnlyFirstRecords(conn);

            //GenerateTestData(conn);

            ShowUsers(conn);
            ShowFlashcards(conn);
            ShowTestResults(conn);
        }
        static void KeepOnlyFirstRecords(NpgsqlConnection conn)
        {
            using (var cmd = new NpgsqlCommand(
                "DELETE FROM users WHERE user_id <> (SELECT MIN(user_id) FROM users);", conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                Console.WriteLine($"Видалено {rowsDeleted} користувачів, крім першого.");
            }

            using (var cmd = new NpgsqlCommand(
                "DELETE FROM flashcards WHERE flashcard_id <> (SELECT MIN(flashcard_id) FROM flashcards);", conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                Console.WriteLine($"Видалено {rowsDeleted} флеш-карток, крім першої.");
            }

            using (var cmd = new NpgsqlCommand(
                "DELETE FROM test_results WHERE result_id <> (SELECT MIN(result_id) FROM test_results);", conn))
            {
                int rowsDeleted = cmd.ExecuteNonQuery();
                Console.WriteLine($"Видалено {rowsDeleted} результатів тестів, крім першого.");
            }
        }


        static void GenerateTestData(NpgsqlConnection conn)
        {
            var random = new Random();
            int count = random.Next(30, 51); 

            for (int i = 1; i <= count; i++)
            {
                string email = $"user{i}@example.com";
                string fullName = $"Тестовий Користувач {i}";
                int points = random.Next(0, 1000);

                using var cmd = new NpgsqlCommand(
                    "INSERT INTO users (email, password_hash, full_name, points) VALUES (@e, @p, @f, @pts) RETURNING user_id",
                    conn);
                cmd.Parameters.AddWithValue("e", email);
                cmd.Parameters.AddWithValue("p", "testhash");
                cmd.Parameters.AddWithValue("f", fullName);
                cmd.Parameters.AddWithValue("pts", points);

                int userId = (int)cmd.ExecuteScalar();

                using var flashCmd = new NpgsqlCommand(
                    "INSERT INTO flashcards (user_id, question, answer) VALUES (@uid, @q, @a)",
                    conn);
                flashCmd.Parameters.AddWithValue("uid", userId);
                flashCmd.Parameters.AddWithValue("q", $"Питання для {fullName}");
                flashCmd.Parameters.AddWithValue("a", $"Відповідь для {fullName}");
                flashCmd.ExecuteNonQuery();

                int totalQuestions = random.Next(5, 11);
                int correctAnswers = random.Next(0, totalQuestions + 1);
                int score = correctAnswers * 10;

                using var testCmd = new NpgsqlCommand(
                    "INSERT INTO test_results (user_id, correct_answers, total_questions, score) VALUES (@uid, @c, @t, @s)",
                    conn);
                testCmd.Parameters.AddWithValue("uid", userId);
                testCmd.Parameters.AddWithValue("c", correctAnswers);
                testCmd.Parameters.AddWithValue("t", totalQuestions);
                testCmd.Parameters.AddWithValue("s", score);
                testCmd.ExecuteNonQuery();
            }


            Console.WriteLine($"Згенеровано {count} користувачів з флеш-картами та результатами тестів.\n");
        }

        static void ShowUsers(NpgsqlConnection conn)
        {
            Console.WriteLine("=== Таблиця USERS ===");
            string query = "SELECT user_id, email, full_name, points, rank, created_at FROM users ORDER BY user_id;";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                Console.WriteLine("Немає користувачів.\n");
                return;
            }

            while (reader.Read())
            {
                Console.WriteLine(
                    $"ID: {reader["user_id"]}, " +
                    $"Email: {reader["email"]}, " +
                    $"Ім'я: {reader["full_name"]}, " +
                    $"Очки: {reader["points"]}, " +
                    $"Ранг: {reader["rank"]}, " +
                    $"Створено: {reader["created_at"]}");
            }
            Console.WriteLine();
        }

        static void ShowFlashcards(NpgsqlConnection conn)
        {
            Console.WriteLine("=== Таблиця FLASHCARDS ===");
            string query = "SELECT flashcard_id, user_id, question, answer, created_at FROM flashcards ORDER BY flashcard_id;";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                Console.WriteLine("Немає карток.\n");
                return;
            }

            while (reader.Read())
            {
                Console.WriteLine(
                    $"ID: {reader["flashcard_id"]}, " +
                    $"UserID: {reader["user_id"]}, " +
                    $"Питання: {reader["question"]}, " +
                    $"Відповідь: {reader["answer"]}, " +
                    $"Створено: {reader["created_at"]}");
            }
            Console.WriteLine();
        }

        static void ShowTestResults(NpgsqlConnection conn)
        {
            Console.WriteLine("=== Таблиця TEST_RESULTS ===");
            string query = "SELECT result_id, user_id, correct_answers, total_questions, score, test_date FROM test_results ORDER BY result_id;";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                Console.WriteLine("Немає результатів тестів.\n");
                return;
            }

            while (reader.Read())
            {
                Console.WriteLine(
                    $"ID: {reader["result_id"]}, " +
                    $"UserID: {reader["user_id"]}, " +
                    $"Правильних: {reader["correct_answers"]}/{reader["total_questions"]}, " +
                    $"Бали: {reader["score"]}, " +
                    $"Дата: {reader["test_date"]}");
            }
            Console.WriteLine();
        }
    }
}
