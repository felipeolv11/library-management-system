namespace Library
{
    class BookLoan
    {
        private static int nextId = 1;
        public int id { get; set; }
        public User user { get; set; }
        public Book book { get; set; }
        public DateTime loanDate { get; set; }
        public DateTime? returnDate { get; set; }

        public void Save()
        {
            using var connection = Database.GetConnection();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO BookLoans (UserName, UserEmail, BookTitle, LoanDate, ReturnDate)
                VALUES ($userName, $userEmail, $bookTitle, $loanDate, $returnDate);
            ";
            command.Parameters.AddWithValue("$userName", user.name);
            command.Parameters.AddWithValue("$userEmail", user.email);
            command.Parameters.AddWithValue("$bookTitle", book.title);
            command.Parameters.AddWithValue("$loanDate", loanDate.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("$returnDate", returnDate?.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }
        
        public BookLoan(User user, Book book, DateTime loanDate)
        {
            this.id = nextId++;
            this.book = book;
            this.user = user;
            this.loanDate = loanDate;
            this.returnDate = null;
        }

        public bool IsReturned()
        {
            return returnDate.HasValue;
        }

        public void RegisterReturn()
        {
            this.returnDate = DateTime.Now;
        }
    }
}