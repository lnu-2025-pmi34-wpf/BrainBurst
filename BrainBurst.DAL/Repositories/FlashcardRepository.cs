namespace BrainBurst.DAL.Repositories
{
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
        /// <param name="context">Контекст бази даних, що буде використовуватися для операцій.</param>
        public FlashcardRepository(ApplicationDbContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// Асинхронно додає нову флеш-картку до бази даних.
        /// </summary>
        /// <param name="f">Сутність <see cref="Flashcard"/> для додавання.</param>
        /// <param name="tags">Список рядків-тегів (логіка наразі не реалізована).</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Додана сутність <see cref="Flashcard"/>.</returns>
        public async Task<Flashcard> AddAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct)
        {
            // Додаємо нову картку.
            // Примітка: Логіка зв'язування тегів з карткою потребує додаткових сутностей (FlashcardTag),
            // які ми не реалізуємо на цьому етапі, тому обробляється лише сама картка.
            this._context.Flashcards.Add(f);
            await this._context.SaveChangesAsync(ct);
            return f;
        }

        /// <summary>
        /// Асинхронно оновлює існуючу флеш-картку.
        /// </summary>
        /// <param name="f">Сутність <see cref="Flashcard"/> з оновленими даними.</param>
        /// <param name="tags">Список рядків-тегів (логіка наразі не реалізована).</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <exception cref="KeyNotFoundException">Виникає, якщо картку не знайдено або користувач не є її власником.</exception>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct)
        {
            var existingCard = await this._context.Flashcards
                                             .FirstOrDefaultAsync(card => card.FlashcardId == f.FlashcardId, ct);

            if (existingCard == null || existingCard.CreatorId != f.CreatorId)
            {
                throw new KeyNotFoundException($"Flashcard with ID {f.FlashcardId} not found or user is not the creator.");
            }

            existingCard.Question = f.Question;
            existingCard.Answer = f.Answer;

            await this._context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Асинхронно видаляє флеш-картку за її ID, перевіряючи право власності.
        /// </summary>
        /// <param name="id">Ідентифікатор флеш-картки для видалення.</param>
        /// <param name="ownerId">Ідентифікатор користувача, який має бути власником картки.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <exception cref="KeyNotFoundException">Виникає, якщо картку не знайдено або користувач не є її власником.</exception>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteAsync(int id, int ownerId, CancellationToken ct)
        {
            var card = await this._context.Flashcards
                                     .FirstOrDefaultAsync(f => f.FlashcardId == id && f.CreatorId == ownerId, ct);

            if (card == null)
            {
                throw new KeyNotFoundException($"Flashcard with ID {id} not found or user is not the owner.");
            }

            this._context.Flashcards.Remove(card);
            await this._context.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Асинхронно отримує одну флеш-картку за її ID.
        /// </summary>
        /// <param name="id">Ідентифікатор флеш-картки.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Знайдена <see cref="Flashcard"/> або null.</returns>
        public async Task<Flashcard?> GetAsync(int id, CancellationToken ct)
        {
            return await this._context.Flashcards.AsNoTracking().FirstOrDefaultAsync(f => f.FlashcardId == id, ct);
        }

        /// <summary>
        /// Асинхронно знаходить флеш-картки, що належать конкретному користувачу, з можливістю пошуку.
        /// </summary>
        /// <param name="ownerId">Ідентифікатор користувача-власника.</param>
        /// <param name="search">Пошуковий рядок (по питанням та відповідям). Може бути null.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="Flashcard"/>, доступний лише для читання.</returns>
        public async Task<IReadOnlyList<Flashcard>> FindAsync(int ownerId, string? search, CancellationToken ct)
        {
            var query = this._context.Flashcards.Where(f => f.CreatorId == ownerId).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Пошук за питанням або відповіддю (регістронезалежний).
                string normalizedSearch = search.Trim().ToLower();
                query = query.Where(f => f.Question.ToLower().Contains(normalizedSearch) ||
                                         f.Answer.ToLower().Contains(normalizedSearch));
            }

            return await query.ToListAsync(ct);
        }
    }
}