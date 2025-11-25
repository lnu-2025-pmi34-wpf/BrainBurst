namespace BrainBurst.DAL.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Представляє сутність "Тег", яка використовується для категоризації флеш-карток.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        public Tag()
        {
            this.Flashcards = new HashSet<Flashcard>();
        }

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
        /// Gets or sets отримує або встановлює ID користувача, який створив цей тег.
        /// </summary>
        public int? CreatorId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює навігаційну властивість до користувача-творця.
        /// </summary>
        public User? Creator { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію флеш-карток, які мають цей тег.
        /// </summary>
        public ICollection<Flashcard> Flashcards { get; set; }
    }
}