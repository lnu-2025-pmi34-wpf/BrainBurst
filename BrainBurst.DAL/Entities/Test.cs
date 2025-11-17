namespace BrainBurst.DAL.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Представляє сутність "Тест", що складається з набору флеш-карток.
    /// </summary>
    public class Test
    {
        /// <summary>
        /// Gets or sets отримує або встановлює унікальний ідентифікатор тесту (Первинний ключ).
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює ID користувача, який створив цей тест (Зовнішній ключ).
        /// </summary>
        public int CreatorId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює навігаційну властивість до користувача-творця.
        /// </summary>
        public User Creator { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію результатів, пов'язаних з цим тестом.
        /// </summary>
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}