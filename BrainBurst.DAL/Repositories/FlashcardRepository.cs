namespace BrainBurst.DAL.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Data;
    using BrainBurst.DAL.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Serilog.Core;

    /// <summary>
    /// Реалізація репозиторію для роботи з сутностями <see cref="Flashcard"/>.
    /// </summary>
    public class FlashcardRepository : IFlashcardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FlashcardRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcardRepository"/> class.
        /// </summary>
        /// <param name="context">Контекст бази даних.</param>
        /// /// <param name="logger">Логер для запису подій.</param>
        public FlashcardRepository(ApplicationDbContext context, ILogger<FlashcardRepository> logger)
        {
            this._context = context;
            this._logger = logger;

            this._logger.LogDebug("FlashcardRepository: Репозиторій карток ініціалізовано.");
        }

        /// <summary>
        /// Асинхронно додає нову флеш-картку та пов'язує її з тегами.
        /// </summary>
        /// <param name="f">Сутність флеш-картки.</param>
        /// <param name="tags">Список назв тегів.</param>
        /// <param name="ct">Токен скасування.</param>
        /// <returns>Створена сутність.</returns>
        public async Task<Flashcard> AddAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct)
        {
            this._logger.LogDebug("AddAsync: Спроба додати нову картку. CreatorId: {CreatorId}", f.CreatorId);
            try
            {
                // 1. Обробка тегів
                if (tags != null)
                {
                    foreach (var tagName in tags)
                    {
                        var normalizedName = tagName.Trim();
                        if (string.IsNullOrEmpty(normalizedName))
                        {
                            this._logger.LogDebug("AddAsync: Пропущено порожній тег.");
                            continue;
                        }

                        // Шукаємо існуючий тег для цього користувача
                        var existingTag = await this._context.Tags
                            .FirstOrDefaultAsync(t => t.Name == normalizedName && t.CreatorId == f.CreatorId, ct);

                        if (existingTag != null)
                        {
                            f.Tags.Add(existingTag);
                            this._logger.LogDebug("AddAsync: Знайдено існуючий тег: {Tag}", normalizedName);
                        }
                        else
                        {
                            // Створюємо новий тег
                            var newTag = new Tag
                            {
                                Name = normalizedName,
                                CreatorId = f.CreatorId,
                            };
                            f.Tags.Add(newTag);
                            this._logger.LogInformation("AddAsync: Створено новий тег: {Tag}", normalizedName);
                        }
                    }
                }

                // 2. Збереження картки (разом з новими тегами та зв'язками)
                this._context.Flashcards.Add(f);
                await this._context.SaveChangesAsync(ct);

                this._logger.LogInformation("AddAsync: Картка {CardId} успішно додана.", f.FlashcardId);

                return f;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "AddAsync: Критична помилка БД при додаванні картки для CreatorId: {CreatorId}", f.CreatorId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно оновлює існуючу флеш-картку та її набір тегів.
        /// </summary>
        /// <param name="f">Сутність флеш-картки з оновленими даними.</param>
        /// <param name="tags">Новий повний список тегів для цієї картки.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
        public async Task UpdateAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct)
        {
            this._logger.LogDebug("UpdateAsync: Спроба оновити картку {CardId}. CreatorId: {CreatorId}", f.FlashcardId, f.CreatorId);

            try
            {
                // 1. Завантажуємо існуючу картку з тегами
                var existingCard = await this._context.Flashcards
                    .Include(c => c.Tags)
                    .FirstOrDefaultAsync(c => c.FlashcardId == f.FlashcardId, ct);

                if (existingCard == null)
                {
                    this._logger.LogWarning("UpdateAsync: Картка {CardId} не знайдена для оновлення.", f.FlashcardId);
                    throw new KeyNotFoundException($"Flashcard with ID {f.FlashcardId} not found.");
                }

                // 2. Оновлюємо текст питання та відповіді
                existingCard.Question = f.Question;
                existingCard.Answer = f.Answer;

                // 3. Очищуємо старі теги
                existingCard.Tags.Clear();
                this._logger.LogDebug("UpdateAsync: Видалено старі теги для {CardId}.", f.FlashcardId);

                // 4. Додаємо нові теги
                if (tags != null)
                {
                    foreach (var tagName in tags)
                    {
                        var normalizedName = tagName.Trim();
                        if (string.IsNullOrEmpty(normalizedName))
                        {
                            continue;
                        }

                        // Шукаємо існуючий тег
                        var existingTag = await this._context.Tags
                            .FirstOrDefaultAsync(t => t.Name == normalizedName && t.CreatorId == f.CreatorId, ct);

                        if (existingTag != null)
                        {
                            existingCard.Tags.Add(existingTag);
                        }
                        else
                        {
                            // Створюємо новий тег
                            var newTag = new Tag
                            {
                                Name = normalizedName,
                                CreatorId = f.CreatorId,
                            };
                            existingCard.Tags.Add(newTag);
                        }
                    }
                }

                // 5. Зберігаємо зміни
                this._context.Flashcards.Update(existingCard);
                await this._context.SaveChangesAsync(ct);

                this._logger.LogInformation("UpdateAsync: Картка {CardId} успішно оновлена.", f.FlashcardId);
            }
            catch (Exception ex)
            {
                if (ex is not KeyNotFoundException)
                {
                    this._logger.LogError(ex, "UpdateAsync: Критична помилка БД при оновленні картки {CardId}", f.FlashcardId); // <-- ЛОГУВАННЯ ПОМИЛКИ
                }

                throw;
            }
        }

        /// <summary>
        /// Асинхронно видаляє флеш-картку за її ID, перевіряючи право власності.
        /// </summary>
        /// <param name="id">ID флеш-картки для видалення.</param>
        /// <param name="ownerId">ID власника (для перевірки безпеки).</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
        public async Task DeleteAsync(int id, int ownerId, CancellationToken ct)
        {
            this._logger.LogDebug("DeleteAsync: Спроба видалити картку {CardId} користувачем {OwnerId}.", id, ownerId);

            try
            {
                var card = await this._context.Flashcards
                    .FirstOrDefaultAsync(f => f.FlashcardId == id && f.CreatorId == ownerId, ct);

                if (card == null)
                {
                    this._logger.LogWarning("DeleteAsync: Видалення невдале. Картка {CardId} не знайдена або CreatorId не співпадає ({OwnerId}).", id, ownerId);
                    throw new KeyNotFoundException($"Flashcard with ID {id} not found.");
                }

                this._context.Flashcards.Remove(card);
                await this._context.SaveChangesAsync(ct);

                this._logger.LogInformation("DeleteAsync: Картка {CardId} успішно видалена.", id);
            }
            catch (Exception ex)
            {
                if (ex is not KeyNotFoundException)
                {
                    this._logger.LogError(ex, "DeleteAsync: Критична помилка БД при видаленні картки {CardId}", id);
                }

                throw;
            }
        }

        /// <summary>
        /// Асинхронно отримує одну флеш-картку за її ID.
        /// </summary>
        /// <param name="id">ID флеш-картки.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Знайдена <see cref="Flashcard"/> або null.</returns>
        public async Task<Flashcard?> GetAsync(int id, CancellationToken ct)
        {
            this._logger.LogDebug("GetAsync: Отримання картки за ID: {CardId}", id);

            try
            {
                var card = await this._context.Flashcards
                    .Include(f => f.Tags)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.FlashcardId == id, ct);

                if (card == null)
                {
                    this._logger.LogDebug("GetAsync: Картка {CardId} не знайдена.", id);
                }

                return card;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "GetAsync: Критична помилка БД при отриманні картки {CardId}", id);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно знаходить флеш-картки за ID власника та (опційно) пошуковим рядком.
        /// </summary>
        /// <param name="ownerId">ID власника карток.</param>
        /// <param name="search">Пошуковий рядок.</param>
        /// <param name="ct">Токен скасування.</param>
        /// <returns>Список карток з завантаженими тегами.</returns>
        public async Task<IReadOnlyList<Flashcard>> FindAsync(int ownerId, string? search, CancellationToken ct)
        {
            this._logger.LogDebug("FindAsync: Пошук карток для власника {OwnerId}. Запит: {Search}", ownerId, search ?? "відсутній");

            try
            {
                var query = this._context.Flashcards
                    .Where(f => f.CreatorId == ownerId)
                    .Include(f => f.Tags)
                    .AsSplitQuery()
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string normalizedSearch = search.Trim().ToLower();
                    query = query.Where(f => f.Question.ToLower().Contains(normalizedSearch) ||
                                             f.Answer.ToLower().Contains(normalizedSearch));
                }

                var result = await query.ToListAsync(ct);

                this._logger.LogInformation("FindAsync: Знайдено {Count} карток для власника {OwnerId}.", result.Count, ownerId);
                return result;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "FindAsync: Критична помилка БД при пошуку карток для власника {OwnerId}.", ownerId);
                throw;
            }
        }
    }
}