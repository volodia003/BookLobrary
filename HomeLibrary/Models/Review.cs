using System;

namespace HomeLibrary.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } // Для отображения
        
        public int UserId { get; set; }
        public string UserName { get; set; } // Для отображения
        
        public int Rating { get; set; } // От 1 до 5
        public string ReviewText { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsRead { get; set; }
        
        public Review()
        {
            ReviewDate = DateTime.Now;
            IsRead = false;
        }
    }
}
