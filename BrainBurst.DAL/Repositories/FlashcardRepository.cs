using BrainBurst.DAL.Abstractions;
using BrainBurst.DAL.Data;
using BrainBurst.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrainBurst.DAL.Repositories
{
    public class FlashcardRepository : IFlashcardRepository
    {
        private readonly ApplicationDbContext _context;

        public FlashcardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Flashcard> AddAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct)
        {
            // Додаємо нову картку. 
            // Примітка: Логіка зв'язування тегів з карткою потребує додаткових сутностей (FlashcardTag),
            // які ми не реалізуємо на цьому етапі, тому обробляється лише сама картка.
            _context.Flashcards.Add(f);
            await _context.SaveChangesAsync(ct);
            return f;
        }

        public async Task UpdateAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct)
        {
            // Знаходимо картку для оновлення.
            var existingCard = await _context.Flashcards
                                             .FirstOrDefaultAsync(card => card.FlashcardId == f.FlashcardId, ct);

            if (existingCard == null || existingCard.CreatorId != f.CreatorId)
            {
                // Важливо перевірити, чи існує картка і чи є поточний користувач її власником.
                throw new KeyNotFoundException($"Flashcard with ID {f.FlashcardId} not found or user is not the creator.");
            }

            // Оновлюємо властивості
            existingCard.Question = f.Question;
            existingCard.Answer = f.Answer;

            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, int ownerId, CancellationToken ct)
        {
            var card = await _context.Flashcards
                                     .FirstOrDefaultAsync(f => f.FlashcardId == id && f.CreatorId == ownerId, ct);

            if (card == null)
            {
                // Якщо картку не знайдено або користувач не є її власником.
                throw new KeyNotFoundException($"Flashcard with ID {id} not found or user is not the owner.");
            }

            _context.Flashcards.Remove(card);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<Flashcard?> GetAsync(int id, CancellationToken ct)
        {
            // Отримання однієї картки за ID.
            return await _context.Flashcards.AsNoTracking().FirstOrDefaultAsync(f => f.FlashcardId == id, ct);
        }

        public async Task<IReadOnlyList<Flashcard>> FindAsync(int ownerId, string? search, CancellationToken ct)
        {
            var query = _context.Flashcards.Where(f => f.CreatorId == ownerId).AsNoTracking();

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