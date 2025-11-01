namespace BrainBurst.DAL.Entities
{
    public class QuestionResult
    {
        public int QuestionResultId { get; set; }

        public int TestResultId { get; set; }
        public TestResult TestResult { get; set; } = null!;

        public int FlashcardId { get; set; }
        public Flashcard Flashcard { get; set; } = null!;

        public string UserInput { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}