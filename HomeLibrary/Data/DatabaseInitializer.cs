using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;

namespace HomeLibrary.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["HomeLibraryDb"].ConnectionString;
            var builder = new SQLiteConnectionStringBuilder(connectionString);
            var dbPath = builder.DataSource;

            // Создать БД если не существует
            bool isNewDatabase = !File.Exists(dbPath);
            
            if (isNewDatabase)
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                if (isNewDatabase)
                {
                    CreateTables(connection);
                    InsertInitialData(connection);
                }
            }
        }

        private static void CreateTables(SQLiteConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                // Таблица пользователей
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL,
                        FullName TEXT NOT NULL,
                        IsAdmin INTEGER NOT NULL DEFAULT 0,
                        CreatedAt TEXT NOT NULL
                    )";
                cmd.ExecuteNonQuery();

                // Таблица авторов
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Authors (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FirstName TEXT,
                        LastName TEXT NOT NULL,
                        Biography TEXT,
                        BirthDate TEXT,
                        Country TEXT
                    )";
                cmd.ExecuteNonQuery();

                // Таблица жанров
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Genres (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE,
                        Description TEXT
                    )";
                cmd.ExecuteNonQuery();

                // Таблица мест хранения
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Locations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        Room TEXT,
                        Shelf TEXT
                    )";
                cmd.ExecuteNonQuery();

                // Таблица книг
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Books (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        ISBN TEXT,
                        PublicationYear INTEGER,
                        Publisher TEXT,
                        PageCount INTEGER,
                        Description TEXT,
                        CoverImagePath TEXT,
                        AuthorId INTEGER,
                        GenreId INTEGER,
                        LocationId INTEGER,
                        Status TEXT NOT NULL DEFAULT 'В наличии',
                        AddedDate TEXT NOT NULL,
                        FOREIGN KEY (AuthorId) REFERENCES Authors(Id),
                        FOREIGN KEY (GenreId) REFERENCES Genres(Id),
                        FOREIGN KEY (LocationId) REFERENCES Locations(Id)
                    )";
                cmd.ExecuteNonQuery();

                // Таблица займов
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Loans (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        BookId INTEGER NOT NULL,
                        BorrowerId INTEGER NOT NULL,
                        LoanDate TEXT NOT NULL,
                        DueDate TEXT,
                        ReturnDate TEXT,
                        Notes TEXT,
                        FOREIGN KEY (BookId) REFERENCES Books(Id),
                        FOREIGN KEY (BorrowerId) REFERENCES Users(Id)
                    )";
                cmd.ExecuteNonQuery();

                // Таблица отзывов
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Reviews (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        BookId INTEGER NOT NULL,
                        UserId INTEGER NOT NULL,
                        Rating INTEGER NOT NULL CHECK(Rating >= 1 AND Rating <= 5),
                        ReviewText TEXT,
                        ReviewDate TEXT NOT NULL,
                        IsRead INTEGER NOT NULL DEFAULT 0,
                        FOREIGN KEY (BookId) REFERENCES Books(Id),
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    )";
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertInitialData(SQLiteConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                // Добавить администратора по умолчанию
                cmd.CommandText = @"INSERT INTO Users (Username, Password, FullName, IsAdmin, CreatedAt) 
                    VALUES ('admin', 'admin123', 'Администратор', 1, @now)";
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                // Добавить нескольких пользователей семьи
                cmd.CommandText = @"INSERT INTO Users (Username, Password, FullName, IsAdmin, CreatedAt) 
                    VALUES ('user1', 'user123', 'Иван Петров', 0, @now)";
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                cmd.CommandText = @"INSERT INTO Users (Username, Password, FullName, IsAdmin, CreatedAt) 
                    VALUES ('user2', 'user123', 'Мария Петрова', 0, @now)";
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                // Добавить несколько жанров
                string[] genres = { "Художественная литература", "Фантастика", "Детектив", "Романтика", 
                    "Биография", "История", "Наука", "Детская литература", "Поэзия", "Классика" };
                foreach (var genre in genres)
                {
                    cmd.CommandText = @"INSERT INTO Genres (Name, Description) VALUES (@name, '')";
                    cmd.Parameters.AddWithValue("@name", genre);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }

                // Добавить несколько авторов
                cmd.CommandText = @"INSERT INTO Authors (FirstName, LastName, Country) VALUES ('Федор', 'Достоевский', 'Россия')";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"INSERT INTO Authors (FirstName, LastName, Country) VALUES ('Лев', 'Толстой', 'Россия')";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"INSERT INTO Authors (FirstName, LastName, Country) VALUES ('Александр', 'Пушкин', 'Россия')";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"INSERT INTO Authors (FirstName, LastName, Country) VALUES ('Антон', 'Чехов', 'Россия')";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"INSERT INTO Authors (FirstName, LastName, Country) VALUES ('Михаил', 'Булгаков', 'Россия')";
                cmd.ExecuteNonQuery();

                // Добавить места хранения
                cmd.CommandText = @"INSERT INTO Locations (Name, Description, Room, Shelf) 
                    VALUES ('Гостиная - Шкаф 1', 'Главный книжный шкаф', 'Гостиная', 'Шкаф 1')";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"INSERT INTO Locations (Name, Description, Room, Shelf) 
                    VALUES ('Спальня - Полка 1', 'Полка у кровати', 'Спальня', 'Полка 1')";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"INSERT INTO Locations (Name, Description, Room, Shelf) 
                    VALUES ('Кабинет - Шкаф 2', 'Рабочий кабинет', 'Кабинет', 'Шкаф 2')";
                cmd.ExecuteNonQuery();

                // Добавить несколько книг для примера
                cmd.CommandText = @"INSERT INTO Books (Title, ISBN, PublicationYear, Publisher, AuthorId, GenreId, LocationId, Status, AddedDate) 
                    VALUES ('Преступление и наказание', '978-5-17-982654-1', 1866, 'АСТ', 1, 10, 1, 'В наличии', @now)";
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                cmd.CommandText = @"INSERT INTO Books (Title, ISBN, PublicationYear, Publisher, AuthorId, GenreId, LocationId, Status, AddedDate) 
                    VALUES ('Война и мир', '978-5-17-982655-8', 1869, 'АСТ', 2, 10, 1, 'В наличии', @now)";
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                cmd.CommandText = @"INSERT INTO Books (Title, ISBN, PublicationYear, Publisher, AuthorId, GenreId, LocationId, Status, AddedDate) 
                    VALUES ('Евгений Онегин', '978-5-17-982656-5', 1833, 'АСТ', 3, 9, 2, 'В наличии', @now)";
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                cmd.CommandText = @"INSERT INTO Books (Title, ISBN, PublicationYear, Publisher, AuthorId, GenreId, LocationId, Status, AddedDate) 
                    VALUES ('Мастер и Маргарита', '978-5-17-982657-2', 1967, 'АСТ', 5, 1, 1, 'В наличии', @now)";
                cmd.Parameters.AddWithValue("@now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
        }
    }
}
