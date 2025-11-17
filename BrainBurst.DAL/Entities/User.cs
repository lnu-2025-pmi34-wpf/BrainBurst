namespace BrainBurst.DAL.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Представляє сутність "Користувач" в базі даних.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets отримує або встановлює унікальний ідентифікатор користувача (Первинний ключ).
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює унікальну адресу електронної пошти користувача.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює хеш пароля користувача.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює повне ім'я користувача (може бути null).
        /// </summary>
        [MaxLength(100)]
        public string? FullName { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює загальну кількість балів (рейтинг) користувача.
        /// </summary>
        public int Points { get; set; } = 0;

        /// <summary>
        /// Gets or sets отримує або встановлює ранг користувача (наприклад, "Початківець").
        /// </summary>
        [MaxLength(20)]
        public string Rank { get; set; } = "Початківець";

        /// <summary>
        /// Gets or sets отримує або встановлює дату та час створення облікового запису.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навігаційні властивості

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію флеш-карток, створених цим користувачем.
        /// </summary>
        public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію тестів, створених цим користувачем.
        /// </summary>
        public ICollection<Test> Tests { get; set; } = new List<Test>();

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію результатів тестів, пройдених цим користувачем.
        /// </summary>
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію тегів, створених цим користувачем.
        /// </summary>
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}