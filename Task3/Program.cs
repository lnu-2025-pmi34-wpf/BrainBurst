using System;
using Npgsql;
using DotNetEnv;

namespace AdoNetPostgresDemo
{
    internal class Program
    {
        static string connectionString;

        static void Main(string[] args)
        {
            Env.Load();
            connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            Console.WriteLine("✅ Підключення успішне!");
        }
    }
}
