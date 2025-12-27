using System;

namespace HomeLibrary.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public int? PublicationYear { get; set; }
        public string Publisher { get; set; }
        public int? PageCount { get; set; }
        public string Description { get; set; }
        public string CoverImagePath { get; set; }
        
        public int? AuthorId { get; set; }
        public string AuthorName { get; set; } // Для отображения
        
        public int? GenreId { get; set; }
        public string GenreName { get; set; } // Для отображения
        
        public int? LocationId { get; set; }
        public string LocationName { get; set; } // Для отображения
        
        public string Status { get; set; } // "В наличии" или "Одолжена"
        public DateTime AddedDate { get; set; }
        
        public Book()
        {
            Status = "В наличии";
            AddedDate = DateTime.Now;
        }
    }
}
