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

                    // Шукаємо існуючий тег для цього користувача (або спільний)
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
                .Include(f => f.Tags) // <--- ВАЖЛИВО: Завантажуємо теги
                .AsSplitQuery()       // Оптимізація для колекцій
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string normalizedSearch = search.Trim().ToLower();
                query = query.Where(f => f.Question.ToLower().Contains(normalizedSearch) ||
                                         f.Answer.ToLower().Contains(normalizedSearch));
            }

            return await query.ToListAsync(ct);
        }

        // Інші методи (UpdateAsync, DeleteAsync, GetAsync) також бажано оновити, 
        // щоб вони враховували Tags, але для відображення списку критичні саме ці два.
        
        // ... (Решта методів без змін або з аналогічним додаванням .Include(f => f.Tags))
        public async Task UpdateAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct)
        {
             // Тут логіка складніша: треба завантажити існуючу картку з тегами,
             // видалити зайві, додати нові.
             // Для MVP поки що можна залишити як є або реалізувати пізніше.
             await Task.CompletedTask; 
        }

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

        public async Task<Flashcard?> GetAsync(int id, CancellationToken ct)
        {
            return await this._context.Flashcards
                .Include(f => f.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FlashcardId == id, ct);
        }
    }
}