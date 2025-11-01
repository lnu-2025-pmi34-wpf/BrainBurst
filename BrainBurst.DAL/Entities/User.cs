using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrainBurst.DAL.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(255)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = null!;

        [MaxLength(100)]
        public string? FullName { get; set; }

        public int Points { get; set; } = 0;

        [MaxLength(20)]
        public string Rank { get; set; } = "Початківець";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навігаційні властивості
        public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
        public ICollection<Test> Tests { get; set; } = new List<Test>();
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}