namespace BrainBurst.BLL.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.BLL.Mapping;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;

    /// <summary>
    /// Реалізація сервісу, що керує логікою флеш-карток.
    /// </summary>
    public class FlashcardService : IFlashcardService
    {
        private readonly IFlashcardRepository _cards;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcardService"/> class.
        /// </summary>
        /// <param name="cards">Репозиторій для доступу до даних флеш-карток.</param>
        public FlashcardService(IFlashcardRepository cards)
        {
            this._cards = cards;
        }

        /// <summary>
        /// Асинхронно створює нову флеш-картку.
        /// </summary>
        /// <param name="creatorId">ID користувача, який створює картку.</param>
        /// <param name="question">Текст питання.</param>
        /// <param name="answer">Текст відповіді.</param>
        /// <param name="tags">Список тегів для картки.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>DTO створеної <see cref="FlashcardDTO"/>.</returns>
        public async Task<FlashcardDTO> CreateAsync(int creatorId, string question, string answer, IEnumerable<string> tags, CancellationToken ct)
        {
            Guard.Text(question, "Питання", max: 4000);
            Guard.Text(answer, "Відповідь", max: 4000);

            var newCard = new Flashcard
            {
                Question = question,
                Answer = answer,
                CreatorId = creatorId,
                CreatedAt = DateTime.UtcNow,
            };

            var savedCard = await this._cards.AddAsync(newCard, tags, ct);

            return savedCard.ToDTO(tags);
        }

        /// <summary>
        /// Асинхронно оновлює існуючу флеш-картку.
        /// </summary>
        /// <param name="id">ID картки, яку потрібно оновити.</param>
        /// <param name="editorId">ID користувача, який виконує оновлення (для перевірки прав).</param>
        /// <param name="question">Новий текст питання.</param>
        /// <param name="answer">Новий текст відповіді.</param>
        /// <param name="tags">Новий список тегів.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>DTO оновленої <see cref="FlashcardDTO"/>.</returns>
        /// <exception cref="KeyNotFoundException">Виникає, якщо картку не знайдено або користувач не є її творцем.</exception>
        public async Task<FlashcardDTO> UpdateAsync(int id, int editorId, string question, string answer, IEnumerable<string> tags, CancellationToken ct)
        {
            Guard.Text(question, "Питання", max: 4000);
            Guard.Text(answer, "Відповідь", max: 4000);

            var existingCard = await this._cards.GetAsync(id, ct);

            if (existingCard == null || existingCard.CreatorId != editorId)
            {
                throw new KeyNotFoundException($"Картку з ID {id} не знайдено або користувач не є її творцем.");
            }

            var updatedCard = new Flashcard
            {
                FlashcardId = id,
                CreatorId = editorId,
                Question = question,
                Answer = answer,
                CreatedAt = existingCard.CreatedAt,
            };

            await this._cards.UpdateAsync(updatedCard, tags, ct);

            return updatedCard.ToDTO(tags);
        }

        /// <summary>
        /// Асинхронно видаляє флеш-картку.
        /// </summary>
        /// <param name="id">ID картки, яку потрібно видалити.</param>
        /// <param name="requesterId">ID користувача, який запитує видалення (для перевірки прав).</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
        public async Task DeleteAsync(int id, int requesterId, CancellationToken ct)
        {
            await this._cards.DeleteAsync(id, requesterId, ct);
        }

        /// <summary>
        /// Асинхронно отримує одну флеш-картку за її ID.
        /// </summary>
        /// <param name="id">ID картки для отримання.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>DTO знайденої <see cref="FlashcardDTO"/> або null, якщо не знайдено.</returns>
        public async Task<FlashcardDTO?> GetAsync(int id, CancellationToken ct)
        {
            var card = await this._cards.GetAsync(id, ct);
            if (card == null)
            {
                return null;
            }

            return card.ToDTO(Array.Empty<string>());
        }

        /// <summary>
        /// Асинхронно отримує список флеш-карток, що належать користувачу.
        /// </summary>
        /// <param name="ownerId">ID користувача, чиї картки потрібно знайти.</param>
        /// <param name="search">Опційний пошуковий рядок для фільтрації.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="FlashcardDTO"/>, доступний лише для читання.</returns>
        public async Task<IReadOnlyList<FlashcardDTO>> ListAsync(int ownerId, string? search, CancellationToken ct)
        {
            var cards = await this._cards.FindAsync(ownerId, search, ct);

            return cards.Select(c => c.ToDTO(Array.Empty<string>())).ToList();
        }
    }
}