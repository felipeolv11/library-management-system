using Microsoft.Data.Sqlite;

namespace Library
{
    public static class Database
    {
        private static readonly string connectionString = "Data Source=library.db";

        public static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static void Initialize()
        {
            using var connection = GetConnection();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Books (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Author TEXT NOT NULL,
                    Available INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS BookLoans (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserName TEXT NOT NULL,
                    UserEmail TEXT NOT NULL,
                    BookTitle TEXT NOT NULL,
                    LoanDate TEXT NOT NULL,
                    ReturnDate TEXT
                );
            ";
            command.ExecuteNonQuery();
        }
    }
}