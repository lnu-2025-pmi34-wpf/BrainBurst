// Клас Guard доступний у глобальному просторі імен

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

    public class FlashcardService : IFlashcardService
    {
        private readonly IFlashcardRepository _cards;

        public FlashcardService(IFlashcardRepository cards)
        {
            this._cards = cards;
        }

        public async Task<FlashcardDTO> CreateAsync(int creatorId, string question, string answer, IEnumerable<string> tags, CancellationToken ct)
        {
            // 1. Валідація вхідних даних
            Guard.Text(question, "Питання", max: 4000);
            Guard.Text(answer, "Відповідь", max: 4000);

            // 2. Створення сутності
            var newCard = new Flashcard
            {
                Question = question,
                Answer = answer,
                CreatorId = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Збереження в DAL. Репозиторій додасть картку і створить зв'язки з тегами.
            var savedCard = await this._cards.AddAsync(newCard, tags, ct);

            // 4. Повернення DTO
            return savedCard.ToDTO(tags);
        }

        public async Task<FlashcardDTO> UpdateAsync(int id, int editorId, string question, string answer, IEnumerable<string> tags, CancellationToken ct)
        {
            // 1. Валідація
            Guard.Text(question, "Питання", max: 4000);
            Guard.Text(answer, "Відповідь", max: 4000);

            // 2. Отримання існуючої картки (для перевірки прав та збереження дати створення)
            var existingCard = await this._cards.GetAsync(id, ct);

            if (existingCard == null || existingCard.CreatorId != editorId)
                throw new KeyNotFoundException($"Картку з ID {id} не знайдено або користувач не є її творцем.");

            // 3. Створення оновленої сутності для передачі в DAL
            var updatedCard = new Flashcard
            {
                FlashcardId = id,
                CreatorId = editorId,
                Question = question,
                Answer = answer,
                CreatedAt = existingCard.CreatedAt
            };

            // 4. Оновлення в DAL
            await this._cards.UpdateAsync(updatedCard, tags, ct);

            // 5. Повернення оновленого DTO
            return updatedCard.ToDTO(tags);
        }

        public async Task DeleteAsync(int id, int requesterId, CancellationToken ct)
        {
            // DAL обробляє перевірку існування та права власності
            await this._cards.DeleteAsync(id, requesterId, ct);
        }

        public async Task<FlashcardDTO?> GetAsync(int id, CancellationToken ct)
        {
            var card = await this._cards.GetAsync(id, ct);
            if (card == null) return null;

            // Примітка: Оскільки ми не реалізували зв'язок Tag/Flashcard, теги будуть пустими.
            return card.ToDTO(Array.Empty<string>());
        }

        public async Task<IReadOnlyList<FlashcardDTO>> ListAsync(int ownerId, string? search, CancellationToken ct)
        {
            var cards = await this._cards.FindAsync(ownerId, search, ct);

            // Мапінг результатів у DTO
            return cards.Select(c => c.ToDTO(Array.Empty<string>())).ToList();
        }
    }
}