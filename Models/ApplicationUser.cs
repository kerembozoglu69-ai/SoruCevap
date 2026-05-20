using Microsoft.AspNetCore.Identity;

namespace SoruCevap_forum_.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? StudentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Points { get; set; } = 0;
        public bool IsSuspended { get; set; } = false;

        // Level calculation
        public string Level => Points switch
        {
            < 100 => "Acemi",
            < 500 => "Bilgin",
            _ => "Üstat"
        };

        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
