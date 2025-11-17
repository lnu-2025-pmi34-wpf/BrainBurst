namespace BrainBurst.DAL.Entities
{
    /// <summary>
    /// Представляє сутність "Результат відповіді на питання" в рамках конкретного тесту.
    /// </summary>
    public class QuestionResult
    {
        /// <summary>
        /// Gets or sets отримує або встановлює унікальний ідентифікатор результату питання (Первинний ключ).
        /// </summary>
        public int QuestionResultId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює ID результату тесту, до якого відноситься це питання (Зовнішній ключ).
        /// </summary>
        public int TestResultId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює навігаційну властивість до батьківського результату тесту.
        /// </summary>
        public TestResult TestResult { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює ID флеш-картки, на яку давалася відповідь (Зовнішній ключ).
        /// </summary>
        public int FlashcardId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює навігаційну властивість до флеш-картки, на яку давалася відповідь.
        /// </summary>
        public Flashcard Flashcard { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює відповідь, яку надав користувач.
        /// </summary>
        public string UserInput { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether отримує або встановлює значення, що вказує, чи була відповідь користувача правильною.
        /// </summary>
        public bool IsCorrect { get; set; }
    }
}