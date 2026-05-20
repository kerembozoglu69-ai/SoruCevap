using System.ComponentModel.DataAnnotations;

namespace SoruCevap_forum_.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Icon { get; set; } // e.g., "fa-calculator"
        
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }
        
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        
        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }

    public class Answer
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }
        
        public int QuestionId { get; set; }
        public virtual Question? Question { get; set; }
        
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }

    public class Like
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }
        
        public int AnswerId { get; set; }
        public virtual Answer? Answer { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
