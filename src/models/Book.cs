namespace Library
{
    class Book
    {
        private static int nextId = 1; 
        public int id { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public bool available { get; set; }

        public void Save()
        {
            using var connection = Database.GetConnection();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Books (Title, Author, Available)
                VALUES ($title, $author, $available);
            ";
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$author", author);
            command.Parameters.AddWithValue("$available", available ? 1 : 0);

            command.ExecuteNonQuery();
        }

        public Book(string title, string author, bool available)
        {
            this.id = nextId++;
            this.title = title;
            this.author = author;
            this.available = available;
        }

        public bool IsAvailable()
        {
            return available;
        }
    }
}