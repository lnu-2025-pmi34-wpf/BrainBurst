namespace BrainBurst.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Представляє сутність "Флеш-картка" в базі даних.
    /// </summary>
    public class Flashcard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Flashcard"/> class.
        /// </summary>
        public Flashcard()
        {
            this.Tags = new HashSet<Tag>();
            this.QuestionResults = new HashSet<QuestionResult>();
        }

        /// <summary>
        /// Gets or sets отримує або встановлює унікальний ідентифікатор флеш-картки (Первинний ключ).
        /// </summary>
        public int FlashcardId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює текст питання флеш-картки.
        /// </summary>
        public string Question { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює текст відповіді на флеш-картку.
        /// </summary>
        public string Answer { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює ID користувача, який створив цю картку (Зовнішній ключ).
        /// </summary>
        public int CreatorId { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює навігаційну властивість до користувача-творця.
        /// </summary>
        public User Creator { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює дату та час створення флеш-картки.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію тегів, пов'язаних з цією карткою.
        /// </summary>
        public ICollection<Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets отримує або встановлює колекцію результатів питань, пов'язаних з цією флеш-карткою.
        /// </summary>
        public ICollection<QuestionResult> QuestionResults { get; set; }
    }
}