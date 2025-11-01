using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BrainBurst.DAL.Entities

{
    public class Flashcard
    {
        public int FlashcardId { get; set; }

        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;

        public int CreatorId { get; set; }
        public User Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<QuestionResult> QuestionResults { get; set; } = new List<QuestionResult>();
    }
}