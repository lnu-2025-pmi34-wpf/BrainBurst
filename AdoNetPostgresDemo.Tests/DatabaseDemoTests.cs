using System;
using System.IO;
using System.Linq;
using Npgsql;
using Xunit;
using AdoNetPostgresDemo;

namespace AdoNetPostgresDemo.Tests
{
    public class DatabaseDemoTests
    {
        /// відкриває підключення, використовуючи змінну середовища CONNECTION_STRING.
        private static NpgsqlConnection OpenConnection()
        {
            var cs = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("CONNECTION_STRING не заданий у змінних середовища.");

            var conn = new NpgsqlConnection(cs);
            conn.Open();
            return conn;
        }

        /// виконуєм дію в транзакції, яка потім відкатується
        private static void InTransaction(Action<DatabaseDemo, NpgsqlConnection> action)
        {
            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                var demo = new DatabaseDemo(conn);
                action(demo, conn);
            }
            finally
            {
                tx.Rollback(); // відкатити всі INSERT/DELETE/UPDATE
            }
        }

        private static int ScalarInt(NpgsqlConnection conn, string sql)
        {
            using var cmd = new NpgsqlCommand(sql, conn);
            var obj = cmd.ExecuteScalar();
            return obj == null || obj is DBNull ? 0 : Convert.ToInt32(obj);
        }

        // ShowUsers

        [Fact]
        public void ShowUsers_PrintsHeader_And_DoesNotThrow()
        {
            InTransaction((demo, conn) =>
            {
                using var sw = new StringWriter();

                demo.ShowUsers(sw);
                var output = sw.ToString();

                Assert.Contains("=== 🧑 Таблиця USERS ===", output);
            });
        }

        [Fact]
        public void ShowUsers_WhenNoUsers_PrintsNoUsersMessage()
        {
            InTransaction((demo, conn) =>
            {
                // видаляємо всіх користувачів у межах транзакції
                using (var cmd = new NpgsqlCommand("DELETE FROM users;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using var sw = new StringWriter();
                demo.ShowUsers(sw);
                var output = sw.ToString();

                Assert.Contains("Немає користувачів", output);
            });
        }

        [Fact]
        public void ShowUsers_NullWriter_ThrowsArgumentNullException()
        {
            InTransaction((demo, _) =>
            {
                Assert.Throws<ArgumentNullException>(() => demo.ShowUsers(null!));
            });
        }

        // ShowFlashcards

        [Fact]
        public void ShowFlashcards_PrintsHeader()
        {
            InTransaction((demo, conn) =>
            {
                using var sw = new StringWriter();

                demo.ShowFlashcards(sw);
                var output = sw.ToString();

                Assert.Contains("=== 🃏 Таблиця FLASHCARDS ===", output);
            });
        }

        [Fact]
        public void ShowFlashcards_WhenNoCards_PrintsNoCardsMessage()
        {
            InTransaction((demo, conn) =>
            {
                using (var cmd = new NpgsqlCommand("DELETE FROM flashcards;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using var sw = new StringWriter();
                demo.ShowFlashcards(sw);
                var output = sw.ToString();

                Assert.Contains("Немає карток", output);
            });
        }

        [Fact]
        public void ShowFlashcards_NullWriter_ThrowsArgumentNullException()
        {
            InTransaction((demo, _) =>
            {
                Assert.Throws<ArgumentNullException>(() => demo.ShowFlashcards(null!));
            });
        }

        // ShowTestResults

        [Fact]
        public void ShowTestResults_PrintsHeader()
        {
            InTransaction((demo, conn) =>
            {
                using var sw = new StringWriter();

                demo.ShowTestResults(sw);
                var output = sw.ToString();

                Assert.Contains("=== 📊 Таблиця TEST_RESULTS ===", output);
            });
        }

        [Fact]
        public void ShowTestResults_WhenNoResults_PrintsNoResultsMessage()
        {
            InTransaction((demo, conn) =>
            {
                using (var cmd = new NpgsqlCommand("DELETE FROM test_results;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using var sw = new StringWriter();
                demo.ShowTestResults(sw);
                var output = sw.ToString();

                Assert.Contains("Немає результатів тестів", output);
            });
        }

        [Fact]
        public void ShowTestResults_NullWriter_ThrowsArgumentNullException()
        {
            InTransaction((demo, _) =>
            {
                Assert.Throws<ArgumentNullException>(() => demo.ShowTestResults(null!));
            });
        }

        // GenerateTestData

        [Fact]
        public void GenerateTestData_InsertsExpectedNumberOfRows()
        {
            InTransaction((demo, conn) =>
            {
                // поточні кількості
                int usersBefore = ScalarInt(conn, "SELECT COUNT(*) FROM users;");
                int flashcardsBefore = ScalarInt(conn, "SELECT COUNT(*) FROM flashcards;");
                int testsBefore = ScalarInt(conn, "SELECT COUNT(*) FROM tests;");
                int resultsBefore = ScalarInt(conn, "SELECT COUNT(*) FROM test_results;");

                demo.GenerateTestData();

                int usersAfter = ScalarInt(conn, "SELECT COUNT(*) FROM users;");
                int flashcardsAfter = ScalarInt(conn, "SELECT COUNT(*) FROM flashcards;");
                int testsAfter = ScalarInt(conn, "SELECT COUNT(*) FROM tests;");
                int resultsAfter = ScalarInt(conn, "SELECT COUNT(*) FROM test_results;");

                int addedUsers = usersAfter - usersBefore;
                int addedCards = flashcardsAfter - flashcardsBefore;
                int addedTests = testsAfter - testsBefore;
                int addedResults = resultsAfter - resultsBefore;

                // кожен користувач -> одна картка, один тест, один результат
                Assert.InRange(addedUsers, 30, 50);
                Assert.Equal(addedUsers, addedCards);
                Assert.Equal(addedUsers, addedTests);
                Assert.Equal(addedUsers, addedResults);
            });
        }

        // KeepOnlyFirstRecords

        [Fact]
        public void KeepOnlyFirstRecords_ReducesTablesToAtMostOneRow()
        {
            InTransaction((demo, conn) =>
            {
                // спочатку додамо трохи тестових даних, щоб точно було >1 рядка
                demo.GenerateTestData();

                demo.KeepOnlyFirstRecords();

                int usersCount = ScalarInt(conn, "SELECT COUNT(*) FROM users;");
                int flashcardsCount = ScalarInt(conn, "SELECT COUNT(*) FROM flashcards;");
                int resultsCount = ScalarInt(conn, "SELECT COUNT(*) FROM test_results;");

                Assert.InRange(usersCount, 0, 1);
                Assert.InRange(flashcardsCount, 0, 1);
                Assert.InRange(resultsCount, 0, 1);
            });
        }

        [Fact]
        public void KeepOnlyFirstRecords_AllowsNullWriter_AndDoesNotThrow()
        {
            InTransaction((demo, conn) =>
            {
                // просто перевіряємо, що виклик із null не викликає помилок
                demo.KeepOnlyFirstRecords(null);
            });
        }

    }
}
