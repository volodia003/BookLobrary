using System;

namespace HomeLibrary.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public User()
        {
            CreatedAt = DateTime.Now;
        }
    }
}
