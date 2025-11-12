namespace Library
{
    class User
    {
        private static int nextId = 1;
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public List<BookLoan> borrowedBooks { get; set; }

        public void Save()
        {
            using var connection = Database.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (Id, Name, Email)
                VALUES ($id, $name, $email);
            ";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$email", email);
            command.ExecuteNonQuery();
        }
        
        public static User FindByEmail(string email)
        {
            using var connection = Database.GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Email FROM Users WHERE Email = $email";
            command.Parameters.AddWithValue("$email", email);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var user = new User(reader.GetString(1), reader.GetString(2));
                user.id = reader.GetInt32(0);
                return user;
            }

            return null;
        }
        
        public User(string name, string email)
        {
            this.name = name;
            this.email = email;
            borrowedBooks = new List<BookLoan>();
        }

        public bool CanBorrow()
        {
            return borrowedBooks.Count < 3;
        }
    }
}