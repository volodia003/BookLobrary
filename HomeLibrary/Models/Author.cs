using System;

namespace HomeLibrary.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Biography { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Country { get; set; }
        
        public string FullName => $"{LastName} {FirstName}".Trim();
    }
}
