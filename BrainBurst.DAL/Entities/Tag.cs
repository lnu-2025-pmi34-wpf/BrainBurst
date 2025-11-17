namespace BrainBurst.DAL.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Представляє сутність "Тег", яка використовується для категоризації флеш-карток.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Gets or sets отримує або встановлює унікальний ідентифікатор тегу (Первинний ключ).
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює назву тегу.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює ID користувача, який створив цей тег (Зовнішній ключ).
        /// Може бути null, якщо тег створений системою.
        /// </summary>
        public int? CreatorId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює навігаційну властивість до користувача-творця.
        /// </summary>
        public User? Creator { get; set; }
    }
}