using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using HomeLibrary.Models;

namespace HomeLibrary.Data
{
    public class DatabaseContext : IDisposable
    {
        private readonly string connectionString;
        private SQLiteConnection connection;

        public DatabaseContext()
        {
            connectionString = ConfigurationManager.ConnectionStrings["HomeLibraryDb"].ConnectionString;
        }

        private SQLiteConnection GetConnection()
        {
            if (connection == null || connection.State != ConnectionState.Open)
            {
                connection = new SQLiteConnection(connectionString);
                connection.Open();
            }
            return connection;
        }

        // === USERS ===
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var cmd = new SQLiteCommand("SELECT * FROM Users ORDER BY FullName", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        IsAdmin = Convert.ToBoolean(reader["IsAdmin"]),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
            }
            return users;
        }

        public User GetUserByUsername(string username)
        {
            using (var cmd = new SQLiteCommand("SELECT * FROM Users WHERE Username = @username", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@username", username);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Username = reader["Username"].ToString(),
                            Password = reader["Password"].ToString(),
                            FullName = reader["FullName"].ToString(),
                            IsAdmin = Convert.ToBoolean(reader["IsAdmin"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };
                    }
                }
            }
            return null;
        }

        public void InsertUser(User user)
        {
            using (var cmd = new SQLiteCommand(@"INSERT INTO Users (Username, Password, FullName, IsAdmin, CreatedAt) 
                VALUES (@username, @password, @fullname, @isadmin, @created)", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.Parameters.AddWithValue("@password", user.Password);
                cmd.Parameters.AddWithValue("@fullname", user.FullName);
                cmd.Parameters.AddWithValue("@isadmin", user.IsAdmin);
                cmd.Parameters.AddWithValue("@created", user.CreatedAt);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateUser(User user)
        {
            using (var cmd = new SQLiteCommand(@"UPDATE Users SET Username=@username, Password=@password, 
                FullName=@fullname, IsAdmin=@isadmin WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", user.Id);
                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.Parameters.AddWithValue("@password", user.Password);
                cmd.Parameters.AddWithValue("@fullname", user.FullName);
                cmd.Parameters.AddWithValue("@isadmin", user.IsAdmin);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteUser(int id)
        {
            using (var cmd = new SQLiteCommand("DELETE FROM Users WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // === AUTHORS ===
        public List<Author> GetAllAuthors()
        {
            var authors = new List<Author>();
            using (var cmd = new SQLiteCommand("SELECT * FROM Authors ORDER BY LastName, FirstName", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    authors.Add(new Author
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Biography = reader["Biography"].ToString(),
                        BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : (DateTime?)null,
                        Country = reader["Country"].ToString()
                    });
                }
            }
            return authors;
        }

        public void InsertAuthor(Author author)
        {
            using (var cmd = new SQLiteCommand(@"INSERT INTO Authors (FirstName, LastName, Biography, BirthDate, Country) 
                VALUES (@firstname, @lastname, @bio, @birth, @country)", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@firstname", author.FirstName ?? "");
                cmd.Parameters.AddWithValue("@lastname", author.LastName ?? "");
                cmd.Parameters.AddWithValue("@bio", author.Biography ?? "");
                cmd.Parameters.AddWithValue("@birth", author.BirthDate.HasValue ? (object)author.BirthDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@country", author.Country ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateAuthor(Author author)
        {
            using (var cmd = new SQLiteCommand(@"UPDATE Authors SET FirstName=@firstname, LastName=@lastname, 
                Biography=@bio, BirthDate=@birth, Country=@country WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", author.Id);
                cmd.Parameters.AddWithValue("@firstname", author.FirstName ?? "");
                cmd.Parameters.AddWithValue("@lastname", author.LastName ?? "");
                cmd.Parameters.AddWithValue("@bio", author.Biography ?? "");
                cmd.Parameters.AddWithValue("@birth", author.BirthDate.HasValue ? (object)author.BirthDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@country", author.Country ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteAuthor(int id)
        {
            using (var cmd = new SQLiteCommand("DELETE FROM Authors WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // === GENRES ===
        public List<Genre> GetAllGenres()
        {
            var genres = new List<Genre>();
            using (var cmd = new SQLiteCommand("SELECT * FROM Genres ORDER BY Name", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    genres.Add(new Genre
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString()
                    });
                }
            }
            return genres;
        }

        public void InsertGenre(Genre genre)
        {
            using (var cmd = new SQLiteCommand(@"INSERT INTO Genres (Name, Description) 
                VALUES (@name, @desc)", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@name", genre.Name);
                cmd.Parameters.AddWithValue("@desc", genre.Description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateGenre(Genre genre)
        {
            using (var cmd = new SQLiteCommand(@"UPDATE Genres SET Name=@name, Description=@desc WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", genre.Id);
                cmd.Parameters.AddWithValue("@name", genre.Name);
                cmd.Parameters.AddWithValue("@desc", genre.Description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteGenre(int id)
        {
            using (var cmd = new SQLiteCommand("DELETE FROM Genres WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // === LOCATIONS ===
        public List<Location> GetAllLocations()
        {
            var locations = new List<Location>();
            using (var cmd = new SQLiteCommand("SELECT * FROM Locations ORDER BY Name", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    locations.Add(new Location
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString(),
                        Room = reader["Room"].ToString(),
                        Shelf = reader["Shelf"].ToString()
                    });
                }
            }
            return locations;
        }

        public void InsertLocation(Location location)
        {
            using (var cmd = new SQLiteCommand(@"INSERT INTO Locations (Name, Description, Room, Shelf) 
                VALUES (@name, @desc, @room, @shelf)", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@name", location.Name);
                cmd.Parameters.AddWithValue("@desc", location.Description ?? "");
                cmd.Parameters.AddWithValue("@room", location.Room ?? "");
                cmd.Parameters.AddWithValue("@shelf", location.Shelf ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateLocation(Location location)
        {
            using (var cmd = new SQLiteCommand(@"UPDATE Locations SET Name=@name, Description=@desc, 
                Room=@room, Shelf=@shelf WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", location.Id);
                cmd.Parameters.AddWithValue("@name", location.Name);
                cmd.Parameters.AddWithValue("@desc", location.Description ?? "");
                cmd.Parameters.AddWithValue("@room", location.Room ?? "");
                cmd.Parameters.AddWithValue("@shelf", location.Shelf ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteLocation(int id)
        {
            using (var cmd = new SQLiteCommand("DELETE FROM Locations WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // === BOOKS ===
        public List<Book> GetAllBooks()
        {
            var books = new List<Book>();
            using (var cmd = new SQLiteCommand(@"SELECT b.*, a.FirstName || ' ' || a.LastName as AuthorName, 
                g.Name as GenreName, l.Name as LocationName 
                FROM Books b 
                LEFT JOIN Authors a ON b.AuthorId = a.Id 
                LEFT JOIN Genres g ON b.GenreId = g.Id 
                LEFT JOIN Locations l ON b.LocationId = l.Id 
                ORDER BY b.Title", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    books.Add(ReadBook(reader));
                }
            }
            return books;
        }

        public List<Book> SearchBooks(string searchText, int? genreId, string status, int? year, bool? hasReview)
        {
            var books = new List<Book>();
            var sql = @"SELECT DISTINCT b.*, a.FirstName || ' ' || a.LastName as AuthorName, 
                g.Name as GenreName, l.Name as LocationName 
                FROM Books b 
                LEFT JOIN Authors a ON b.AuthorId = a.Id 
                LEFT JOIN Genres g ON b.GenreId = g.Id 
                LEFT JOIN Locations l ON b.LocationId = l.Id 
                LEFT JOIN Reviews r ON b.Id = r.BookId 
                WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                sql += " AND (b.Title LIKE @search OR a.FirstName LIKE @search OR a.LastName LIKE @search OR b.ISBN LIKE @search)";
            }
            if (genreId.HasValue)
            {
                sql += " AND b.GenreId = @genreId";
            }
            if (!string.IsNullOrEmpty(status))
            {
                sql += " AND b.Status = @status";
            }
            if (year.HasValue)
            {
                sql += " AND b.PublicationYear = @year";
            }
            if (hasReview.HasValue)
            {
                if (hasReview.Value)
                    sql += " AND r.Id IS NOT NULL";
                else
                    sql += " AND r.Id IS NULL";
            }

            sql += " ORDER BY b.Title";

            using (var cmd = new SQLiteCommand(sql, GetConnection()))
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");
                if (genreId.HasValue)
                    cmd.Parameters.AddWithValue("@genreId", genreId.Value);
                if (!string.IsNullOrEmpty(status))
                    cmd.Parameters.AddWithValue("@status", status);
                if (year.HasValue)
                    cmd.Parameters.AddWithValue("@year", year.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(ReadBook(reader));
                    }
                }
            }
            return books;
        }

        private Book ReadBook(SQLiteDataReader reader)
        {
            return new Book
            {
                Id = Convert.ToInt32(reader["Id"]),
                Title = reader["Title"].ToString(),
                ISBN = reader["ISBN"].ToString(),
                PublicationYear = reader["PublicationYear"] != DBNull.Value ? Convert.ToInt32(reader["PublicationYear"]) : (int?)null,
                Publisher = reader["Publisher"].ToString(),
                PageCount = reader["PageCount"] != DBNull.Value ? Convert.ToInt32(reader["PageCount"]) : (int?)null,
                Description = reader["Description"].ToString(),
                CoverImagePath = reader["CoverImagePath"].ToString(),
                AuthorId = reader["AuthorId"] != DBNull.Value ? Convert.ToInt32(reader["AuthorId"]) : (int?)null,
                AuthorName = reader["AuthorName"].ToString(),
                GenreId = reader["GenreId"] != DBNull.Value ? Convert.ToInt32(reader["GenreId"]) : (int?)null,
                GenreName = reader["GenreName"].ToString(),
                LocationId = reader["LocationId"] != DBNull.Value ? Convert.ToInt32(reader["LocationId"]) : (int?)null,
                LocationName = reader["LocationName"].ToString(),
                Status = reader["Status"].ToString(),
                AddedDate = Convert.ToDateTime(reader["AddedDate"])
            };
        }

        public void InsertBook(Book book)
        {
            using (var cmd = new SQLiteCommand(@"INSERT INTO Books (Title, ISBN, PublicationYear, Publisher, PageCount, 
                Description, CoverImagePath, AuthorId, GenreId, LocationId, Status, AddedDate) 
                VALUES (@title, @isbn, @year, @pub, @pages, @desc, @cover, @author, @genre, @location, @status, @added)", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@title", book.Title);
                cmd.Parameters.AddWithValue("@isbn", book.ISBN ?? "");
                cmd.Parameters.AddWithValue("@year", book.PublicationYear.HasValue ? (object)book.PublicationYear.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@pub", book.Publisher ?? "");
                cmd.Parameters.AddWithValue("@pages", book.PageCount.HasValue ? (object)book.PageCount.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@desc", book.Description ?? "");
                cmd.Parameters.AddWithValue("@cover", book.CoverImagePath ?? "");
                cmd.Parameters.AddWithValue("@author", book.AuthorId.HasValue ? (object)book.AuthorId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@genre", book.GenreId.HasValue ? (object)book.GenreId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@location", book.LocationId.HasValue ? (object)book.LocationId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@status", book.Status);
                cmd.Parameters.AddWithValue("@added", book.AddedDate);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateBook(Book book)
        {
            using (var cmd = new SQLiteCommand(@"UPDATE Books SET Title=@title, ISBN=@isbn, PublicationYear=@year, 
                Publisher=@pub, PageCount=@pages, Description=@desc, CoverImagePath=@cover, AuthorId=@author, 
                GenreId=@genre, LocationId=@location, Status=@status WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", book.Id);
                cmd.Parameters.AddWithValue("@title", book.Title);
                cmd.Parameters.AddWithValue("@isbn", book.ISBN ?? "");
                cmd.Parameters.AddWithValue("@year", book.PublicationYear.HasValue ? (object)book.PublicationYear.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@pub", book.Publisher ?? "");
                cmd.Parameters.AddWithValue("@pages", book.PageCount.HasValue ? (object)book.PageCount.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@desc", book.Description ?? "");
                cmd.Parameters.AddWithValue("@cover", book.CoverImagePath ?? "");
                cmd.Parameters.AddWithValue("@author", book.AuthorId.HasValue ? (object)book.AuthorId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@genre", book.GenreId.HasValue ? (object)book.GenreId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@location", book.LocationId.HasValue ? (object)book.LocationId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@status", book.Status);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteBook(int id)
        {
            using (var cmd = new SQLiteCommand("DELETE FROM Books WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // === LOANS ===
        public List<Loan> GetAllLoans()
        {
            var loans = new List<Loan>();
            using (var cmd = new SQLiteCommand(@"SELECT l.*, b.Title as BookTitle, u.FullName as BorrowerName 
                FROM Loans l 
                INNER JOIN Books b ON l.BookId = b.Id 
                INNER JOIN Users u ON l.BorrowerId = u.Id 
                ORDER BY l.LoanDate DESC", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    loans.Add(ReadLoan(reader));
                }
            }
            return loans;
        }

        public List<Loan> GetActiveLoans()
        {
            var loans = new List<Loan>();
            using (var cmd = new SQLiteCommand(@"SELECT l.*, b.Title as BookTitle, u.FullName as BorrowerName 
                FROM Loans l 
                INNER JOIN Books b ON l.BookId = b.Id 
                INNER JOIN Users u ON l.BorrowerId = u.Id 
                WHERE l.ReturnDate IS NULL 
                ORDER BY l.LoanDate DESC", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    loans.Add(ReadLoan(reader));
                }
            }
            return loans;
        }

        private Loan ReadLoan(SQLiteDataReader reader)
        {
            return new Loan
            {
                Id = Convert.ToInt32(reader["Id"]),
                BookId = Convert.ToInt32(reader["BookId"]),
                BookTitle = reader["BookTitle"].ToString(),
                BorrowerId = Convert.ToInt32(reader["BorrowerId"]),
                BorrowerName = reader["BorrowerName"].ToString(),
                LoanDate = Convert.ToDateTime(reader["LoanDate"]),
                DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : (DateTime?)null,
                ReturnDate = reader["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(reader["ReturnDate"]) : (DateTime?)null,
                Notes = reader["Notes"].ToString()
            };
        }

        public void InsertLoan(Loan loan)
        {
            using (var cmd = new SQLiteCommand(@"INSERT INTO Loans (BookId, BorrowerId, LoanDate, DueDate, ReturnDate, Notes) 
                VALUES (@bookid, @borrowerid, @loandate, @duedate, @returndate, @notes)", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@bookid", loan.BookId);
                cmd.Parameters.AddWithValue("@borrowerid", loan.BorrowerId);
                cmd.Parameters.AddWithValue("@loandate", loan.LoanDate);
                cmd.Parameters.AddWithValue("@duedate", loan.DueDate.HasValue ? (object)loan.DueDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@returndate", loan.ReturnDate.HasValue ? (object)loan.ReturnDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", loan.Notes ?? "");
                cmd.ExecuteNonQuery();
            }

            // Обновить статус книги
            using (var cmd = new SQLiteCommand("UPDATE Books SET Status='Одолжена' WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", loan.BookId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateLoan(Loan loan)
        {
            using (var cmd = new SQLiteCommand(@"UPDATE Loans SET BookId=@bookid, BorrowerId=@borrowerid, 
                LoanDate=@loandate, DueDate=@duedate, ReturnDate=@returndate, Notes=@notes WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", loan.Id);
                cmd.Parameters.AddWithValue("@bookid", loan.BookId);
                cmd.Parameters.AddWithValue("@borrowerid", loan.BorrowerId);
                cmd.Parameters.AddWithValue("@loandate", loan.LoanDate);
                cmd.Parameters.AddWithValue("@duedate", loan.DueDate.HasValue ? (object)loan.DueDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@returndate", loan.ReturnDate.HasValue ? (object)loan.ReturnDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", loan.Notes ?? "");
                cmd.ExecuteNonQuery();
            }

            // Обновить статус книги
            if (loan.ReturnDate.HasValue)
            {
                using (var cmd = new SQLiteCommand("UPDATE Books SET Status='В наличии' WHERE Id=@id", GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@id", loan.BookId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteLoan(int id)
        {
            // Получить BookId перед удалением
            int bookId = 0;
            using (var cmd = new SQLiteCommand("SELECT BookId FROM Loans WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                if (result != null)
                    bookId = Convert.ToInt32(result);
            }

            using (var cmd = new SQLiteCommand("DELETE FROM Loans WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            // Обновить статус книги
            if (bookId > 0)
            {
                using (var cmd = new SQLiteCommand("UPDATE Books SET Status='В наличии' WHERE Id=@id", GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@id", bookId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // === REVIEWS ===
        public List<Review> GetAllReviews()
        {
            var reviews = new List<Review>();
            using (var cmd = new SQLiteCommand(@"SELECT r.*, b.Title as BookTitle, u.FullName as UserName 
                FROM Reviews r 
                INNER JOIN Books b ON r.BookId = b.Id 
                INNER JOIN Users u ON r.UserId = u.Id 
                ORDER BY r.ReviewDate DESC", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    reviews.Add(ReadReview(reader));
                }
            }
            return reviews;
        }

        public List<Review> GetReviewsByBook(int bookId)
        {
            var reviews = new List<Review>();
            using (var cmd = new SQLiteCommand(@"SELECT r.*, b.Title as BookTitle, u.FullName as UserName 
                FROM Reviews r 
                INNER JOIN Books b ON r.BookId = b.Id 
                INNER JOIN Users u ON r.UserId = u.Id 
                WHERE r.BookId = @bookid 
                ORDER BY r.ReviewDate DESC", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@bookid", bookId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reviews.Add(ReadReview(reader));
                    }
                }
            }
            return reviews;
        }

        private Review ReadReview(SQLiteDataReader reader)
        {
            return new Review
            {
                Id = Convert.ToInt32(reader["Id"]),
                BookId = Convert.ToInt32(reader["BookId"]),
                BookTitle = reader["BookTitle"].ToString(),
                UserId = Convert.ToInt32(reader["UserId"]),
                UserName = reader["UserName"].ToString(),
                Rating = Convert.ToInt32(reader["Rating"]),
                ReviewText = reader["ReviewText"].ToString(),
                ReviewDate = Convert.ToDateTime(reader["ReviewDate"]),
                IsRead = Convert.ToBoolean(reader["IsRead"])
            };
        }

        public void InsertReview(Review review)
        {
            using (var cmd = new SQLiteCommand(@"INSERT INTO Reviews (BookId, UserId, Rating, ReviewText, ReviewDate, IsRead) 
                VALUES (@bookid, @userid, @rating, @text, @date, @isread)", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@bookid", review.BookId);
                cmd.Parameters.AddWithValue("@userid", review.UserId);
                cmd.Parameters.AddWithValue("@rating", review.Rating);
                cmd.Parameters.AddWithValue("@text", review.ReviewText ?? "");
                cmd.Parameters.AddWithValue("@date", review.ReviewDate);
                cmd.Parameters.AddWithValue("@isread", review.IsRead);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateReview(Review review)
        {
            using (var cmd = new SQLiteCommand(@"UPDATE Reviews SET BookId=@bookid, UserId=@userid, Rating=@rating, 
                ReviewText=@text, ReviewDate=@date, IsRead=@isread WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", review.Id);
                cmd.Parameters.AddWithValue("@bookid", review.BookId);
                cmd.Parameters.AddWithValue("@userid", review.UserId);
                cmd.Parameters.AddWithValue("@rating", review.Rating);
                cmd.Parameters.AddWithValue("@text", review.ReviewText ?? "");
                cmd.Parameters.AddWithValue("@date", review.ReviewDate);
                cmd.Parameters.AddWithValue("@isread", review.IsRead);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteReview(int id)
        {
            using (var cmd = new SQLiteCommand("DELETE FROM Reviews WHERE Id=@id", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // === STATISTICS ===
        public Dictionary<string, int> GetGenreStatistics()
        {
            var stats = new Dictionary<string, int>();
            using (var cmd = new SQLiteCommand(@"SELECT g.Name, COUNT(b.Id) as Count 
                FROM Genres g 
                LEFT JOIN Books b ON g.Id = b.GenreId 
                GROUP BY g.Name 
                ORDER BY Count DESC", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    stats[reader["Name"].ToString()] = Convert.ToInt32(reader["Count"]);
                }
            }
            return stats;
        }

        public Dictionary<int, int> GetRatingStatistics()
        {
            var stats = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
                stats[i] = 0;

            using (var cmd = new SQLiteCommand(@"SELECT Rating, COUNT(*) as Count 
                FROM Reviews 
                GROUP BY Rating 
                ORDER BY Rating", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    stats[Convert.ToInt32(reader["Rating"])] = Convert.ToInt32(reader["Count"]);
                }
            }
            return stats;
        }

        public Dictionary<string, int> GetReadingStatisticsByMonth(int year)
        {
            var stats = new Dictionary<string, int>();
            using (var cmd = new SQLiteCommand(@"SELECT strftime('%m', ReviewDate) as Month, COUNT(*) as Count 
                FROM Reviews 
                WHERE IsRead = 1 AND strftime('%Y', ReviewDate) = @year 
                GROUP BY Month 
                ORDER BY Month", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@year", year.ToString());
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var month = reader["Month"].ToString();
                        stats[month] = Convert.ToInt32(reader["Count"]);
                    }
                }
            }
            return stats;
        }

        public Dictionary<int, int> GetReadingStatisticsByYear()
        {
            var stats = new Dictionary<int, int>();
            using (var cmd = new SQLiteCommand(@"SELECT strftime('%Y', ReviewDate) as Year, COUNT(*) as Count 
                FROM Reviews 
                WHERE IsRead = 1 
                GROUP BY Year 
                ORDER BY Year", GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    stats[Convert.ToInt32(reader["Year"])] = Convert.ToInt32(reader["Count"]);
                }
            }
            return stats;
        }

        public void Dispose()
        {
            connection?.Close();
            connection?.Dispose();
        }
    }
}
