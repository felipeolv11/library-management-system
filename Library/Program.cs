using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Book> books = new List<Book>();
            List<User> users = new List<User>();

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
                        ErrorMessage("\ninvalid name or email, please try again");
                        continue;
                    }

                    User user = users.FirstOrDefault(u => u.email == userEmail);

                    if (user == null)
                    {
                        user = new User(userName, userEmail);
                        users.Add(user);
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

                                List<Book> foundBooks = new List<Book>();

                                foreach (var book in books)
                                {
                                    if (book.title.Equals(bookTitle))
                                    {
                                        foundBooks.Add(book);
                                    }
                                }

                                if (foundBooks.Count > 0)
                                {
                                    foreach (var bookFound in foundBooks)
                                    {
                                        Console.WriteLine("id: {0} | book found: {1} by {2}", bookFound.id, bookFound.title, bookFound.author);
                                        Thread.Sleep(100);
                                    }

                                    Console.ReadLine();
                                }

                                else
                                {
                                    ErrorMessage("no books found with that title");
                                }

                                break;

                            case 2:
                                LibraryTag();

                                if (books.Any(b => b.IsAvailable()))
                                {
                                    Console.WriteLine("available books\n");

                                    foreach (var book in books.Where(b => b.IsAvailable()))
                                    {
                                        Console.WriteLine("id: {0} | title: {1} | author: {2}", book.id, book.title, book.author);
                                        Thread.Sleep(100);
                                    }

                                    Console.ReadLine();
                                    break;
                                }

                                else
                                {
                                    ErrorMessage("\nno books in the system");
                                    break;
                                }

                            case 3:
                                LibraryTag();

                                Console.Write("enter the book id: ");
                                int borrowBookId = int.Parse(Console.ReadLine());

                                bool bookExistsBorrow = false;

                                foreach (var book in books)
                                {
                                    if (book.id.Equals(borrowBookId))
                                    {
                                        if (book.IsAvailable() && users.Last().CanBorrow())
                                        {
                                            bookExistsBorrow = true;

                                            book.available = false;
                                            user.borrowedBooks.Add(new BookLoan(user, book, DateTime.Now));
                                            SuccessMessage("\nbook borrowed successfully");

                                            break;
                                        }

                                        else if (book.available == false)
                                        {
                                            ErrorMessage("\nbook is not available for borrowing");

                                            break;
                                        }

                                        else if (!user.CanBorrow())
                                        {
                                            ErrorMessage("\nyou have already borrowed 3 books, please return one before borrowing another");

                                            break;
                                        }
                                    }
                                }

                                if (!bookExistsBorrow)
                                {
                                    ErrorMessage("\nbook not found");
                                }

                                break;

                            case 4:
                                LibraryTag();

                                Console.Write("enter the book id: ");

                                if (!int.TryParse(Console.ReadLine(), out int returnBookId))
                                {
                                    ErrorMessage("\ninvalid id, please try again");
                                    continue;
                                }

                                bool bookExistsReturn = false;

                                foreach (var loan in user.borrowedBooks)
                                {
                                    if (loan.book.id.Equals(returnBookId) && !loan.IsReturned())
                                    {
                                        loan.RegisterReturn();
                                        loan.book.available = true;
                                        SuccessMessage("\nbook returned successfully");

                                        bookExistsReturn = true;
                                        break;
                                    }

                                    else if (loan.book.id.Equals(returnBookId) && loan.IsReturned())
                                    {
                                        ErrorMessage("\nbook already returned");
                                        bookExistsReturn = true;
                                        break;
                                    }
                                }

                                if (!bookExistsReturn)
                                {
                                    ErrorMessage("\nbook not found in your borrowed books");
                                }

                                break;

                            case 5:
                                LibraryTag();

                                if (user.borrowedBooks.Count > 0)
                                {
                                    Console.WriteLine("{0}'s borrowed books\n", user.name);

                                    foreach (var loan in user.borrowedBooks)
                                    {
                                        if (!loan.book.available)
                                        {
                                            Console.WriteLine("id: {0} | book: {1} | loan date: {2}", loan.id, loan.book.title, loan.loanDate);
                                            Thread.Sleep(100);
                                        }

                                        Console.ReadLine();
                                    }

                                    break;
                                }

                                else
                                {
                                    ErrorMessage("\nno borrowed books");
                                    break;
                                }

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

                                Console.Write("enter the books title: ");
                                string bookTitle = Console.ReadLine();

                                Console.Write("enter the books author: ");
                                string bookAuthor = Console.ReadLine();

                                if (string.IsNullOrWhiteSpace(bookTitle) || string.IsNullOrWhiteSpace(bookAuthor))
                                {
                                    ErrorMessage("\ninvalid title or author, please try again");
                                    continue;
                                }

                                bool bookExistsAdd = false;

                                foreach (var book in books)
                                {
                                    if (book.title.Equals(bookTitle) && book.author.Equals(bookAuthor))
                                    {
                                        ErrorMessage("\nbook already exists in the system");
                                        bookExistsAdd = true;
                                        break;
                                    }
                                }

                                if (!bookExistsAdd)
                                {
                                    Book newBook = new Book(bookTitle, bookAuthor, true);
                                    books.Add(newBook);

                                    SuccessMessage($"\nbook added successfully");
                                }

                                break;

                            case 2:
                                LibraryTag();

                                Console.Write("enter the book id: ");

                                if (!int.TryParse(Console.ReadLine(), out int removeBookId))
                                {
                                    ErrorMessage("\ninvalid id, please try again");
                                    continue;
                                }

                                bool bookExistsRemove = false;

                                foreach (var book in books)
                                {
                                    if (book.id.Equals(removeBookId))
                                    {
                                        books.Remove(book);

                                        SuccessMessage("\nbook removed successfully");
                                        bookExistsRemove = true;
                                        break;
                                    }
                                }

                                if (!bookExistsRemove)
                                {
                                    ErrorMessage("\nbook not found");
                                }

                                break;

                            case 3:
                                LibraryTag();

                                Console.Write("enter the book id: ");

                                if (!int.TryParse(Console.ReadLine(), out int editBookId))
                                {
                                    ErrorMessage("\ninvalid id, please try again");
                                    continue;
                                }

                                bool bookExistsEdit = false;

                                foreach (var book in books)
                                {
                                    if (book.id.Equals(editBookId))
                                    {
                                        LibraryTag();

                                        Console.WriteLine("enter which one you want to update");
                                        Console.WriteLine("1. title");
                                        Console.WriteLine("2. author\n");

                                        if (!int.TryParse(Console.ReadLine(), out int editChoice))
                                        {
                                            ErrorMessage("\ninvalid choice, please try again");
                                            continue;
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
                                                    continue;
                                                }

                                                bool duplicateExistsTitle = books.Any(b => b.title == newTitle && b.author == book.author && b.id != book.id);

                                                if (duplicateExistsTitle)
                                                {
                                                    ErrorMessage("\nbook with this title and author already exists.");
                                                    continue;
                                                }

                                                else
                                                {
                                                    book.title = newTitle;
                                                    SuccessMessage("\ntitle updated successfully");
                                                    break;
                                                }

                                            case 2:
                                                LibraryTag();

                                                Console.Write("\nenter new author: ");
                                                string newAuthor = Console.ReadLine();

                                                if (string.IsNullOrWhiteSpace(newAuthor))
                                                {
                                                    ErrorMessage("\ninvalid author, please try again");
                                                    continue;
                                                }

                                                bool duplicateExistsAuthor = books.Any(b => b.title == book.title && b.author == newAuthor && b.id != book.id);

                                                if (duplicateExistsAuthor)
                                                {
                                                    ErrorMessage("\nbook with this title and author already exists.");
                                                    continue;
                                                }

                                                else
                                                {
                                                    book.author = newAuthor;
                                                    SuccessMessage("\nauthor updated successfully");
                                                    break;
                                                }

                                            default:
                                                ErrorMessage("\ninvalid choice, please try again");
                                                break;
                                        }

                                        bookExistsEdit = true;
                                        break;
                                    }
                                }

                                if (!bookExistsEdit)
                                {
                                    ErrorMessage("\nbook not found");
                                }

                                break;

                            case 4:
                                LibraryTag();

                                if (books.Count > 0)
                                {
                                    Console.WriteLine("all books (including borrowed)\n");

                                    foreach (var book in books)
                                    {
                                        Console.WriteLine("id: {0} | title: {1} | author: {2} | available: {3}", book.id, book.title, book.author, book.IsAvailable() ? "yes" : "no");
                                        Thread.Sleep(100);
                                    }

                                    Console.ReadLine();
                                }

                                else
                                {
                                    ErrorMessage("\nno books in the system");
                                }

                                break;

                            case 5:
                                LibraryTag();

                                if (users.Count > 0)
                                {
                                    Console.WriteLine("all customers and their borrowings\n");

                                    foreach (var user in users)
                                    {
                                        Console.WriteLine("\ncustomer: {0}, email: {1}", user.name, user.email);

                                        if (user.borrowedBooks.Count > 0)
                                        {
                                            Console.WriteLine("borrowed books:");
                                            foreach (var loan in user.borrowedBooks)
                                            {
                                                Console.WriteLine("id: {0} | book: {1} | loan date: {2} | returned: {3}", loan.id, loan.book.title, loan.loanDate, loan.IsReturned() ? "yes" : "no");
                                                Thread.Sleep(100);
                                            }
                                        }

                                        else
                                        {
                                            ErrorMessage("\nno borrowed books for this customer");
                                        }
                                    }

                                    Console.ReadLine();
                                    break;
                                }

                                else
                                {
                                    ErrorMessage("\nno customers in the system");
                                    break;
                                }

                            case 6:
                                LibraryTag();

                                Console.Write("enter the costumer name: ");
                                string customerName = Console.ReadLine();

                                if (string.IsNullOrWhiteSpace(customerName))
                                {
                                    ErrorMessage("\ninvalid name, please try again");
                                    continue;
                                }

                                List<User> customersSameName = new List<User>();

                                foreach (var user in users)
                                {
                                    if (user.name.Equals(customerName))
                                    {
                                        customersSameName.Add(user);
                                    }
                                }

                                if (customersSameName.Count > 0)
                                {
                                    foreach (var customer in customersSameName)
                                    {
                                        Console.WriteLine("\ncustomer: {0}, email: {1}", customer.name, customer.email);

                                        if (customer.borrowedBooks.Count > 0)
                                        {
                                            Console.WriteLine("borrowing history:");
                                            foreach (var loan in customer.borrowedBooks)
                                            {
                                                Console.WriteLine("id: {0} | book: {1} | loan date: {2} | returned: {3}", loan.id, loan.book.title, loan.loanDate, loan.IsReturned() ? "yes" : "no");
                                                Thread.Sleep(100);
                                            }
                                        }

                                        else
                                        {
                                            ErrorMessage("\nno borrowing history for this customer");
                                        }
                                    }

                                    Console.ReadLine();
                                }

                                else
                                {
                                    ErrorMessage("\nno customers found with that name");
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