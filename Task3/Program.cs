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
            Console.WriteLine("✅ Підключення успішне!\n");

            ShowUsers(conn);
            ShowFlashcards(conn);
            ShowTestResults(conn);
        }

        static void ShowUsers(NpgsqlConnection conn)
        {
            Console.WriteLine("=== 🧑 Таблиця USERS ===");
            string query = "SELECT user_id, email, full_name, points, rank, created_at FROM users ORDER BY user_id;";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                Console.WriteLine("❌ Немає користувачів.\n");
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
            Console.WriteLine("=== 🃏 Таблиця FLASHCARDS ===");
            string query = "SELECT flashcard_id, user_id, question, answer, created_at FROM flashcards ORDER BY flashcard_id;";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                Console.WriteLine("❌ Немає карток.\n");
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
            Console.WriteLine("=== 📊 Таблиця TEST_RESULTS ===");
            string query = "SELECT result_id, user_id, correct_answers, total_questions, score, test_date FROM test_results ORDER BY result_id;";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                Console.WriteLine("❌ Немає результатів тестів.\n");
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
