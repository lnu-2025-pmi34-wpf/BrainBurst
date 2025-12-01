namespace BrainBurst.BLL.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.BLL.Mapping;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Реалізація сервісу, що керує логікою флеш-карток.
    /// </summary>
    public class FlashcardService : IFlashcardService
    {
        private readonly IFlashcardRepository _cards;
        private readonly ILogger<FlashcardService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcardService"/> class.
        /// </summary>
        /// <param name="cards">Репозиторій для доступу до даних флеш-карток.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public FlashcardService(IFlashcardRepository cards, ILogger<FlashcardService> logger)
        {
            this._cards = cards;
            this._logger = logger;

            this._logger.LogDebug("FlashcardService: Сервіс карток ініціалізовано.");
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
            this._logger.LogInformation("CreateAsync: Спроба створення картки користувачем {CreatorId}.", creatorId);
            try
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

                this._logger.LogInformation("CreateAsync: Картка {CardId} успішно створена.", savedCard.FlashcardId);

                return savedCard.ToDTO(tags);
            }
            catch (ArgumentException ex)
            {
                this._logger.LogWarning(ex, "CreateAsync: Помилка валідації при створенні картки користувачем {CreatorId}.", creatorId);
                throw;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "CreateAsync: Критична помилка при створенні картки користувачем {CreatorId}.", creatorId);
                throw;
            }
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
            this._logger.LogInformation("UpdateAsync: Спроба оновлення картки {CardId} користувачем {EditorId}.", id, editorId);
            try
            {
                Guard.Text(question, "Питання", max: 4000);
                Guard.Text(answer, "Відповідь", max: 4000);

                var existingCard = await this._cards.GetAsync(id, ct);

                if (existingCard == null || existingCard.CreatorId != editorId)
                {
                    this._logger.LogError("UpdateAsync: Відмовлено в доступі або картка не знайдена. Картка {CardId}, Користувач {EditorId}.", id, editorId);
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

                this._logger.LogInformation("UpdateAsync: Картка {CardId} успішно оновлена.", id);

                return updatedCard.ToDTO(tags);
            }
            catch (Exception ex)
            {
                if (ex is not KeyNotFoundException)
                {
                    this._logger.LogError(ex, "UpdateAsync: Критична помилка при оновленні картки {CardId}.", id);
                }

                throw;
            }
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
            this._logger.LogInformation("DeleteAsync: Запит на видалення картки {CardId} користувачем {RequesterId}.", id, requesterId);

            try
            {
                await this._cards.DeleteAsync(id, requesterId, ct);
                this._logger.LogInformation("DeleteAsync: Картка {CardId} успішно видалена.", id);
            }
            catch (KeyNotFoundException ex)
            {
                this._logger.LogWarning(ex, "DeleteAsync: Видалення невдале. Картка {CardId} не знайдена або немає прав.", id);
                throw;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "DeleteAsync: Критична помилка при видаленні картки {CardId}.", id);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно отримує одну флеш-картку за її ID.
        /// </summary>
        /// <param name="id">ID картки для отримання.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>DTO знайденої <see cref="FlashcardDTO"/> або null, якщо не знайдено.</returns>
        public async Task<FlashcardDTO?> GetAsync(int id, CancellationToken ct)
        {
            this._logger.LogDebug("GetAsync: Отримання картки за ID: {CardId}", id);

            var card = await this._cards.GetAsync(id, ct);
            if (card == null)
            {
                this._logger.LogDebug("GetAsync: Картка {CardId} не знайдена.", id);
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
            this._logger.LogDebug("ListAsync: Отримання списку карток для користувача {OwnerId}. Пошук: {Search}", ownerId, search ?? "відсутній");

            try
            {
                var cards = await this._cards.FindAsync(ownerId, search, ct);
                this._logger.LogInformation("ListAsync: Знайдено {Count} карток для користувача {OwnerId}.", cards.Count, ownerId); // <-- УСПІХ

                return cards.Select(c => c.ToDTO(c.Tags.Select(t => t.Name))).ToList();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "ListAsync: Критична помилка при отриманні списку карток для користувача {OwnerId}.", ownerId);
                throw;
            }
        }
    }
}