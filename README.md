# 📚 Library  

> A simple **console-based Library Management System** built with **C#** and **SQLite**.

![screenshot](./assets/screenshot1.png)

![screenshot](./assets/screenshot2.png)

![screenshot](./assets/screenshot3.png)

## 📌 Note  

A simple project made to improve my skills in object-oriented programming, data persistence, and CRUD logic in a console app using C#. Now, all data (books, users, and loans) is stored in a SQLite database, allowing permanent data storage between sessions.


## 💡 Features  

- Distinct user roles: **Customer** and **Employee**

### 👤 Customer
- View available books  
- Borrow and return books  
- View personal borrowing history  

### 🧑‍💼 Employee
- Add, remove, and edit book details  
- View all books (including borrowed ones)  
- View all customers and their borrowings  
- Access borrowing history  

---

## 🗃️ Database  

- **SQLite** is used to store and manage all data.  
- The database file (library.db) is automatically created and managed through the Database.cs class.  
- Tables:
  - Users
  - Books
  - BookLoans


## 🛠️ Built With  

- C# (.NET)
- SQLite