using System;

namespace HomeLibrary.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } // Для отображения
        
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } // Для отображения
        
        public DateTime LoanDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Notes { get; set; }
        
        public bool IsReturned => ReturnDate.HasValue;
        public bool IsOverdue => !IsReturned && DueDate.HasValue && DueDate.Value < DateTime.Now;
        
        public Loan()
        {
            LoanDate = DateTime.Now;
        }
    }
}
