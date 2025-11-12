namespace Library
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.Initialize();

            while (true)
            {
                LibraryTag();

                Console.WriteLine("1. customer");
                Console.WriteLine("2. employee");
                Console.WriteLine("3. exit\n");

                if (!int.TryParse(Console.ReadLine(), out int userChoice) || (userChoice < 1 || userChoice > 3))
                {
                    ErrorMessage("\ninvalid choice, please try again");
                    continue;
                }

                if (userChoice == 1)
                {
                    LibraryTag();

                    Console.Write("enter your name: ");
                    string userName = Console.ReadLine();

                    Console.Write("enter your email: ");
                    string userEmail = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userEmail))
                    {
                        Console.WriteLine("\ninvalid name or email, please try again");
                        return;
                    }

                    User user = User.FindByEmail(userEmail);

                    if (user == null)
                    {
                        user = new User(userName, userEmail);
                        user.Save();
                    }

                    int customerChoice = 0;
                    while (customerChoice != 6)
                    {
                        LibraryTag();

                        Console.Write("> user: ");

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0}\n", user.name);

                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("1. search a book by title");
                        Console.WriteLine("2. list available books");
                        Console.WriteLine("3. borrow a book");
                        Console.WriteLine("4. return a book");
                        Console.WriteLine("5. view my borrowed books");
                        Console.WriteLine("6. exit\n");

                        if (!int.TryParse(Console.ReadLine(), out customerChoice))
                        {
                            ErrorMessage("\ninvalid choice, please try again");
                            continue;
                        }

                        switch (customerChoice)
                        {
                            case 1:
                                LibraryTag();

                                Console.Write("enter the book title: ");
                                string bookTitle = Console.ReadLine();
                                Console.Write("\n");

                                if (string.IsNullOrWhiteSpace(bookTitle))
                                {
                                    ErrorMessage("\ninvalid title, please try again");
                                    continue;
                                }

                                using (var connection = Database.GetConnection())
                                {
                                    var command = connection.CreateCommand();
                                    command.CommandText = @"
                                        SELECT Id, Title, Author, Available
                                        FROM Books
                                        WHERE Title LIKE $title;
                                    ";
                                    command.Parameters.AddWithValue("$title", "%" + bookTitle + "%");

                                    using (var reader = command.ExecuteReader())
                                    {
                                        bool foundAny = false;

                                        while (reader.Read())
                                        {
                                            foundAny = true;
                                            int id = reader.GetInt32(0);
                                            string title = reader.GetString(1);
                                            string author = reader.GetString(2);
                                            bool available = reader.GetInt32(3) == 1;

                                            Console.WriteLine($"id: {id} | {title} by {author} | available: {(available ? "yes" : "no")}");
                                            Thread.Sleep(100);
                                        }

                                        if (!foundAny)
                                            ErrorMessage("no books found with that title");
                                        else
                                            Console.ReadLine();
                                    }
                                }

                                break;

                            case 2:
                                LibraryTag();

                                using (var connection = Database.GetConnection())
                                {
                                    var command = connection.CreateCommand();
                                    command.CommandText = @"
                                        SELECT Id, Title, Author
                                        FROM Books
                                        WHERE Available = 1;
                                    ";

                                    using (var reader = command.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            Console.WriteLine("available books:\n");

                                            while (reader.Read())
                                            {
                                                int id = reader.GetInt32(0);
                                                string title = reader.GetString(1);
                                                string author = reader.GetString(2);

                                                Console.WriteLine($"id: {id} | title: {title} | author: {author}");
                                                Thread.Sleep(100);
                                            }

                                            Console.ReadLine();
                                        }

                                        else
                                        {
                                            ErrorMessage("\nno books in the system");
                                        }
                                    }
                                }

                                break;

                            case 3:
                                LibraryTag();

                                Console.Write("enter the book id: ");
                                if (!int.TryParse(Console.ReadLine(), out int borrowBookId))
                                {
                                    ErrorMessage("\ninvalid id, please try again");
                                    break;
                                }

                                using (var connection = Database.GetConnection())
                                {
                                    var selectCmd = connection.CreateCommand();
                                    selectCmd.CommandText = "SELECT Title, Author, Available FROM Books WHERE Id = $id;";
                                    selectCmd.Parameters.AddWithValue("$id", borrowBookId);

                                    using var reader = selectCmd.ExecuteReader();

                                    if (!reader.Read())
                                    {
                                        ErrorMessage("\nbook not found");
                                        break;
                                    }

                                    string title = reader.GetString(0);
                                    string author = reader.GetString(1);
                                    bool available = reader.GetInt32(2) == 1;

                                    if (!available)
                                    {
                                        ErrorMessage("\nbook is not available for borrowing");
                                        break;
                                    }

                                    if (!user.CanBorrow())
                                    {
                                        ErrorMessage("\nyou have already borrowed 3 books, please return one before borrowing another");
                                        break;
                                    }

                                    var loanCmd = connection.CreateCommand();
                                    loanCmd.CommandText = @"
                                        INSERT INTO BookLoans (UserName, UserEmail, BookTitle, LoanDate)
                                        VALUES ($userName, $userEmail, $bookTitle, $loanDate);
                                    ";
                                    loanCmd.Parameters.AddWithValue("$userName", user.name);
                                    loanCmd.Parameters.AddWithValue("$userEmail", user.email);
                                    loanCmd.Parameters.AddWithValue("$bookTitle", title);
                                    loanCmd.Parameters.AddWithValue("$loanDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    loanCmd.ExecuteNonQuery();

                                    var updateCmd = connection.CreateCommand();
                                    updateCmd.CommandText = "UPDATE Books SET Available = 0 WHERE Id = $id;";
                                    updateCmd.Parameters.AddWithValue("$id", borrowBookId);
                                    updateCmd.ExecuteNonQuery();

                                    SuccessMessage($"\nbook '{title}' borrowed successfully!");
                                }

                                break;

                            case 4:
                                LibraryTag();

                                Console.Write("enter the book id: ");
                                if (!int.TryParse(Console.ReadLine(), out int returnBookId))
                                {
                                    ErrorMessage("\ninvalid id, please try again");
                                    break;
                                }

                                using (var connection = Database.GetConnection())
                                {
                                    var checkBookCmd = connection.CreateCommand();
                                    checkBookCmd.CommandText = "SELECT Title FROM Books WHERE Id = $id;";
                                    checkBookCmd.Parameters.AddWithValue("$id", returnBookId);

                                    bookTitle = checkBookCmd.ExecuteScalar() as string;

                                    if (bookTitle == null)
                                    {
                                        ErrorMessage("\nbook not found");
                                        break;
                                    }

                                    var checkLoanCmd = connection.CreateCommand();
                                    checkLoanCmd.CommandText = @"
                                        SELECT Id FROM BookLoans
                                        WHERE BookTitle = $bookTitle
                                        AND UserEmail = $userEmail
                                        AND ReturnDate IS NULL;
                                    ";
                                    checkLoanCmd.Parameters.AddWithValue("$bookTitle", bookTitle);
                                    checkLoanCmd.Parameters.AddWithValue("$userEmail", user.email);

                                    var loanIdObj = checkLoanCmd.ExecuteScalar();

                                    if (loanIdObj == null)
                                    {
                                        ErrorMessage("\nthis book is not currently borrowed by you or has already been returned");
                                        break;
                                    }

                                    int loanId = Convert.ToInt32(loanIdObj);

                                    var updateLoanCmd = connection.CreateCommand();
                                    updateLoanCmd.CommandText = "UPDATE BookLoans SET ReturnDate = $returnDate WHERE Id = $loanId;";
                                    updateLoanCmd.Parameters.AddWithValue("$returnDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    updateLoanCmd.Parameters.AddWithValue("$loanId", loanId);
                                    updateLoanCmd.ExecuteNonQuery();

                                    var updateBookCmd = connection.CreateCommand();
                                    updateBookCmd.CommandText = "UPDATE Books SET Available = 1 WHERE Id = $id;";
                                    updateBookCmd.Parameters.AddWithValue("$id", returnBookId);
                                    updateBookCmd.ExecuteNonQuery();

                                    SuccessMessage($"\nbook '{bookTitle}' returned successfully!");
                                }

                                break;

                            case 5:
                                LibraryTag();

                                using (var connection = Database.GetConnection())
                                {
                                    var command = connection.CreateCommand();
                                    command.CommandText = @"
                                        SELECT Id, BookTitle, LoanDate, ReturnDate
                                        FROM BookLoans
                                        WHERE UserEmail = $userEmail;
                                    ";
                                    command.Parameters.AddWithValue("$userEmail", user.email);

                                    using (var reader = command.ExecuteReader())
                                    {
                                        if (!reader.HasRows)
                                        {
                                            ErrorMessage("\nno borrowed books");
                                            break;
                                        }

                                        Console.WriteLine($"{user.name}'s borrowed books\n");

                                        while (reader.Read())
                                        {
                                            int id = reader.GetInt32(0);
                                            string title = reader.GetString(1);
                                            string loanDate = reader.GetString(2);
                                            string? returnDate = reader.IsDBNull(3) ? null : reader.GetString(3);

                                            if (returnDate == null)
                                            {
                                                Console.WriteLine($"id: {id} | book: {title} | loan date: {loanDate}");
                                            }

                                            else
                                            {
                                                Console.WriteLine($"id: {id} | book: {title} | loan date: {loanDate} | returned on: {returnDate}");
                                            }

                                            Thread.Sleep(100);
                                        }

                                        Console.ReadLine();
                                    }
                                }

                                break;

                            case 6:
                                break;

                            default:
                                ErrorMessage("\ninvalid choice, please try again");
                                break;
                        }
                    }
                }

                else if (userChoice == 2)
                {
                    int employeeChoice = 0;
                    while (employeeChoice != 7)
                    {
                        LibraryTag();

                        Console.WriteLine("> admin dashboard\n");
                        Console.WriteLine("1. add new book");
                        Console.WriteLine("2. remove book from system");
                        Console.WriteLine("3. edit book information");
                        Console.WriteLine("4. list all books (including borrowed)");
                        Console.WriteLine("5. view all customers and their borrowings");
                        Console.WriteLine("6. view borrowing history");
                        Console.WriteLine("7. exit\n");

                        if (!int.TryParse(Console.ReadLine(), out employeeChoice))
                        {
                            ErrorMessage("\ninvalid choice, please try again");
                            continue;
                        }

                        switch (employeeChoice)
                        {
                            case 1:
                                LibraryTag();

                                Console.Write("enter the book title: ");
                                string bookTitle = Console.ReadLine();

                                Console.Write("enter the book author: ");
                                string bookAuthor = Console.ReadLine();

                                if (string.IsNullOrWhiteSpace(bookTitle) || string.IsNullOrWhiteSpace(bookAuthor))
                                {
                                    ErrorMessage("\ninvalid title or author, please try again");
                                    break;
                                }

                                using (var connection = Database.GetConnection())
                                {
                                    var checkCmd = connection.CreateCommand();
                                    checkCmd.CommandText = @"
                                        SELECT COUNT(*) FROM Books
                                        WHERE Title = $title AND Author = $author;
                                    ";
                                    checkCmd.Parameters.AddWithValue("$title", bookTitle);
                                    checkCmd.Parameters.AddWithValue("$author", bookAuthor);

                                    long count = (long)checkCmd.ExecuteScalar();

                                    if (count > 0)
                                    {
                                        ErrorMessage("\nbook already exists in the system");
                                        break;
                                    }

                                    var insertCmd = connection.CreateCommand();
                                    insertCmd.CommandText = @"
                                        INSERT INTO Books (Title, Author, Available)
                                        VALUES ($title, $author, 1);
                                    ";
                                    insertCmd.Parameters.AddWithValue("$title", bookTitle);
                                    insertCmd.Parameters.AddWithValue("$author", bookAuthor);
                                    insertCmd.ExecuteNonQuery();
                                }

                                SuccessMessage($"\nbook added successfully");
                                break;

                            case 2:
                                LibraryTag();

                                Console.Write("enter the book id: ");
                                if (!int.TryParse(Console.ReadLine(), out int removeBookId))
                                {
                                    ErrorMessage("\ninvalid id, please try again");
                                    break;
                                }

                                using (var connection = Database.GetConnection())
                                {
                                    var checkCmd = connection.CreateCommand();
                                    checkCmd.CommandText = "SELECT COUNT(*) FROM Books WHERE Id = $id;";
                                    checkCmd.Parameters.AddWithValue("$id", removeBookId);

                                    long count = (long)checkCmd.ExecuteScalar();

                                    if (count == 0)
                                    {
                                        ErrorMessage("\nbook not found");
                                        break;
                                    }

                                    var deleteCmd = connection.CreateCommand();
                                    deleteCmd.CommandText = "DELETE FROM Books WHERE Id = $id;";
                                    deleteCmd.Parameters.AddWithValue("$id", removeBookId);
                                    deleteCmd.ExecuteNonQuery();
                                }

                                SuccessMessage("\nbook removed successfully");
                                break;

                            case 3:
                                LibraryTag();

                                Console.Write("enter the book id: ");
                                if (!int.TryParse(Console.ReadLine(), out int editBookId))
                                {
                                    ErrorMessage("\ninvalid id, please try again");
                                    break;
                                }

                                using (var connection = Database.GetConnection())
                                {
                                    var checkCmd = connection.CreateCommand();
                                    checkCmd.CommandText = "SELECT Title, Author FROM Books WHERE Id = $id;";
                                    checkCmd.Parameters.AddWithValue("$id", editBookId);

                                    using var reader = checkCmd.ExecuteReader();
                                    if (!reader.Read())
                                    {
                                        ErrorMessage("\nbook not found");
                                        break;
                                    }

                                    string currentTitle = reader.GetString(0);
                                    string currentAuthor = reader.GetString(1);

                                    LibraryTag();
                                    Console.WriteLine("enter which one you want to update");
                                    Console.WriteLine("1. title");
                                    Console.WriteLine("2. author\n");

                                    if (!int.TryParse(Console.ReadLine(), out int editChoice))
                                    {
                                        ErrorMessage("\ninvalid choice, please try again");
                                        break;
                                    }

                                    switch (editChoice)
                                    {
                                        case 1:
                                            LibraryTag();
                                            Console.Write("\nenter new title: ");
                                            string newTitle = Console.ReadLine();

                                            if (string.IsNullOrWhiteSpace(newTitle))
                                            {
                                                ErrorMessage("\ninvalid title, please try again");
                                                break;
                                            }

                                            var dupTitleCmd = connection.CreateCommand();
                                            dupTitleCmd.CommandText = @"
                                                SELECT COUNT(*) FROM Books 
                                                WHERE Title = $title AND Author = $author AND Id != $id;
                                            ";
                                            dupTitleCmd.Parameters.AddWithValue("$title", newTitle);
                                            dupTitleCmd.Parameters.AddWithValue("$author", currentAuthor);
                                            dupTitleCmd.Parameters.AddWithValue("$id", editBookId);

                                            long countTitle = (long)dupTitleCmd.ExecuteScalar();
                                            if (countTitle > 0)
                                            {
                                                ErrorMessage("\nbook with this title and author already exists.");
                                                break;
                                            }

                                            var updateTitleCmd = connection.CreateCommand();
                                            updateTitleCmd.CommandText = "UPDATE Books SET Title = $title WHERE Id = $id;";
                                            updateTitleCmd.Parameters.AddWithValue("$title", newTitle);
                                            updateTitleCmd.Parameters.AddWithValue("$id", editBookId);
                                            updateTitleCmd.ExecuteNonQuery();

                                            SuccessMessage("\ntitle updated successfully");
                                            break;

                                        case 2:
                                            LibraryTag();
                                            Console.Write("\nenter new author: ");
                                            string newAuthor = Console.ReadLine();

                                            if (string.IsNullOrWhiteSpace(newAuthor))
                                            {
                                                ErrorMessage("\ninvalid author, please try again");
                                                break;
                                            }

                                            var dupAuthorCmd = connection.CreateCommand();
                                            dupAuthorCmd.CommandText = @"
                                                SELECT COUNT(*) FROM Books 
                                                WHERE Title = $title AND Author = $author AND Id != $id;
                                            ";
                                            dupAuthorCmd.Parameters.AddWithValue("$title", currentTitle);
                                            dupAuthorCmd.Parameters.AddWithValue("$author", newAuthor);
                                            dupAuthorCmd.Parameters.AddWithValue("$id", editBookId);

                                            long countAuthor = (long)dupAuthorCmd.ExecuteScalar();
                                            if (countAuthor > 0)
                                            {
                                                ErrorMessage("\nbook with this title and author already exists.");
                                                break;
                                            }

                                            var updateAuthorCmd = connection.CreateCommand();
                                            updateAuthorCmd.CommandText = "UPDATE Books SET Author = $author WHERE Id = $id;";
                                            updateAuthorCmd.Parameters.AddWithValue("$author", newAuthor);
                                            updateAuthorCmd.Parameters.AddWithValue("$id", editBookId);
                                            updateAuthorCmd.ExecuteNonQuery();

                                            SuccessMessage("\nauthor updated successfully");
                                            break;

                                        default:
                                            ErrorMessage("\ninvalid choice, please try again");
                                            break;
                                    }
                                }

                                break;

                            case 4:
                                LibraryTag();

                                using (var connection = Database.GetConnection())
                                {
                                    var cmd = connection.CreateCommand();
                                    cmd.CommandText = "SELECT Id, Title, Author, Available FROM Books;";

                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (!reader.HasRows)
                                        {
                                            ErrorMessage("\nno books in the system");
                                            break;
                                        }

                                        Console.WriteLine("all books (including borrowed)\n");

                                        while (reader.Read())
                                        {
                                            int id = reader.GetInt32(0);
                                            string title = reader.GetString(1);
                                            string author = reader.GetString(2);
                                            bool available = reader.GetBoolean(3);

                                            Console.WriteLine(
                                                "id: {0} | title: {1} | author: {2} | available: {3}",
                                                id, title, author, available ? "yes" : "no"
                                            );
                                            Thread.Sleep(100);
                                        }

                                        Console.ReadLine();
                                    }
                                }

                                break;

                            case 5:
                                LibraryTag();

                                using (var connection = Database.GetConnection())
                                {
                                    var cmd = connection.CreateCommand();
                                    cmd.CommandText = @"
                                        SELECT 
                                            UserName, 
                                            UserEmail, 
                                            BookTitle, 
                                            LoanDate, 
                                            ReturnDate
                                        FROM BookLoans
                                        ORDER BY UserName;
                                    ";

                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (!reader.HasRows)
                                        {
                                            ErrorMessage("\nno customers or borrowings found in the system");
                                            break;
                                        }

                                        Console.WriteLine("all customers and their borrowings\n");

                                        string currentUser = null;

                                        while (reader.Read())
                                        {
                                            string userName = reader.GetString(0);
                                            string userEmail = reader.GetString(1);
                                            bookTitle = reader.GetString(2);
                                            string loanDate = reader.GetString(3);
                                            string returnDate = reader.IsDBNull(4) ? "not returned" : reader.GetString(4);

                                            if (currentUser != userName)
                                            {
                                                Console.WriteLine($"\ncustomer: {userName}, email: {userEmail}");
                                                Console.WriteLine("borrowed books:");
                                                currentUser = userName;
                                            }

                                            Console.WriteLine($"- book: {bookTitle} | loan date: {loanDate} | returned: {(returnDate == "not returned" ? "no" : "yes")}");
                                            Thread.Sleep(100);
                                        }

                                        Console.ReadLine();
                                    }
                                }

                                break;

                            case 6:
                                LibraryTag();

                                Console.Write("enter the customer name: ");
                                string customerName = Console.ReadLine();

                                if (string.IsNullOrWhiteSpace(customerName))
                                {
                                    ErrorMessage("\ninvalid name, please try again");
                                    continue;
                                }

                                using (var connection = Database.GetConnection())
                                {
                                    var cmd = connection.CreateCommand();
                                    cmd.CommandText = @"
                                        SELECT 
                                            UserName, 
                                            UserEmail, 
                                            BookTitle, 
                                            LoanDate, 
                                            ReturnDate
                                        FROM BookLoans
                                        WHERE UserName = $userName
                                        ORDER BY LoanDate DESC;
                                    ";
                                    cmd.Parameters.AddWithValue("$userName", customerName);

                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (!reader.HasRows)
                                        {
                                            ErrorMessage("\nno customers found with that name or no borrowing history");
                                            break;
                                        }

                                        string userEmail = null;
                                        Console.WriteLine($"\ncustomer: {customerName}");

                                        while (reader.Read())
                                        {
                                            if (userEmail == null)
                                                userEmail = reader.GetString(1);

                                            bookTitle = reader.GetString(2);
                                            string loanDate = reader.GetString(3);
                                            string returnDate = reader.IsDBNull(4) ? "not returned" : reader.GetString(4);

                                            Console.WriteLine($"- book: {bookTitle} | loan date: {loanDate} | returned: {(returnDate == "not returned" ? "no" : "yes")}");
                                            Thread.Sleep(100);
                                        }

                                        Console.WriteLine($"\nemail: {userEmail}");
                                        Console.ReadLine();
                                    }
                                }

                                break;

                            case 7:
                                break;

                            default:
                                ErrorMessage("\ninvalid choice, please try again");
                                break;
                        }
                    }
                }

                else if (userChoice == 3)
                {
                    Console.WriteLine("\nexiting...");
                    Thread.Sleep(1000);
                    return;
                }
            }
        }

        static public void LibraryTag()
        {
            Console.Clear();

            string asciiArt = " ___       ___  ________  ________  ________  ________      ___    ___ \r\n|\\  \\     |\\  \\|\\   __  \\|\\   __  \\|\\   __  \\|\\   __  \\    |\\  \\  /  /|\r\n\\ \\  \\    \\ \\  \\ \\  \\|\\ /\\ \\  \\|\\  \\ \\  \\|\\  \\ \\  \\|\\  \\   \\ \\  \\/  / /\r\n \\ \\  \\    \\ \\  \\ \\   __  \\ \\   _  _\\ \\   __  \\ \\   _  _\\   \\ \\    / / \r\n  \\ \\  \\____\\ \\  \\ \\  \\|\\  \\ \\  \\\\  \\\\ \\  \\ \\  \\ \\  \\\\  \\|   \\/  /  /  \r\n   \\ \\_______\\ \\__\\ \\_______\\ \\__\\\\ _\\\\ \\__\\ \\__\\ \\__\\\\ _\\ __/  / /    \r\n    \\|_______|\\|__|\\|_______|\\|__|\\|__|\\|__|\\|__|\\|__|\\|__|\\___/ /     \r\n                                                          \\|___|/      ";

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(asciiArt);

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static public void ErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ReadLine();
        }

        public static void SuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green; 
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }
}