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

    /// <summary>
    /// Реалізація репозиторію для роботи з сутностями <see cref="Flashcard"/>.
    /// </summary>
    public class FlashcardRepository : IFlashcardRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcardRepository"/> class.
        /// </summary>
        /// <param name="context">Контекст бази даних.</param>
        public FlashcardRepository(ApplicationDbContext context)
        {
            this._context = context;
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
            // 1. Обробка тегів
            if (tags != null)
            {
                foreach (var tagName in tags)
                {
                    var normalizedName = tagName.Trim();
                    if (string.IsNullOrEmpty(normalizedName))
                    {
                        continue;
                    }

                    // Шукаємо існуючий тег для цього користувача
                    var existingTag = await this._context.Tags
                        .FirstOrDefaultAsync(t => t.Name == normalizedName && t.CreatorId == f.CreatorId, ct);

                    if (existingTag != null)
                    {
                        f.Tags.Add(existingTag);
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
                    }
                }
            }

            // 2. Збереження картки (разом з новими тегами та зв'язками)
            this._context.Flashcards.Add(f);
            await this._context.SaveChangesAsync(ct);

            return f;
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
            // 1. Завантажуємо існуючу картку з тегами
            var existingCard = await this._context.Flashcards
                .Include(c => c.Tags)
                .FirstOrDefaultAsync(c => c.FlashcardId == f.FlashcardId, ct);

            if (existingCard == null)
            {
                throw new KeyNotFoundException($"Flashcard with ID {f.FlashcardId} not found.");
            }

            // 2. Оновлюємо текст питання та відповіді
            existingCard.Question = f.Question;
            existingCard.Answer = f.Answer;

            // 3. Очищуємо старі теги
            existingCard.Tags.Clear();

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
            var card = await this._context.Flashcards
                .FirstOrDefaultAsync(f => f.FlashcardId == id && f.CreatorId == ownerId, ct);

            if (card == null)
            {
                throw new KeyNotFoundException($"Flashcard with ID {id} not found.");
            }

            this._context.Flashcards.Remove(card);
            await this._context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Асинхронно отримує одну флеш-картку за її ID.
        /// </summary>
        /// <param name="id">ID флеш-картки.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Знайдена <see cref="Flashcard"/> або null.</returns>
        public async Task<Flashcard?> GetAsync(int id, CancellationToken ct)
        {
            return await this._context.Flashcards
                .Include(f => f.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FlashcardId == id, ct);
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

            return await query.ToListAsync(ct);
        }
    }
}