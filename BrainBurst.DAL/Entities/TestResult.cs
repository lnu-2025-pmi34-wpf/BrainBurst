namespace BrainBurst.DAL.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Представляє сутність "Результат тесту", що зберігає підсумок проходження тесту користувачем.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Gets or sets отримує або встановлює унікальний ідентифікатор результату тесту (Первинний ключ).
        /// </summary>
        public int TestResultId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює ID тесту, до якого відноситься цей результат (Зовнішній ключ).
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює навігаційну властивість до пов'язаного тесту.
        /// </summary>
        public Test Test { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює ID користувача, який пройшов тест (Зовнішній ключ).
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює навігаційну властивість до користувача, який пройшов тест.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює відсоток правильних відповідей (наприклад, 80.50).
        /// </summary>
        [Column(TypeName = "numeric(5,2)")]
        public decimal CorrectAnswersPercent { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює кількість балів, отриманих за тест.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює дату та час проходження тесту.
        /// </summary>
        public DateTime TestDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію детальних відповідей на питання для цього результату тесту.
        /// </summary>
        public ICollection<QuestionResult> QuestionResults { get; set; } = new List<QuestionResult>();
    }
}