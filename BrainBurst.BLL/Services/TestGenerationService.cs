#pragma warning disable SA1200
using BrainBurst.BLL.Mapping;
#pragma warning restore SA1200

namespace BrainBurst.BLL.Services;

/// <summary>
/// Реалізація сервісу, що відповідає за генерацію флеш-карток з тексту.
/// </summary>
public sealed class TestGenerationService : ITestGenerationService
{
    private readonly IQuizGenerator _ai;
    private readonly IFlashcardRepository _cards;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestGenerationService"/> class.
    /// </summary>
    /// <param name="ai">Сервіс-генератор квізів (наприклад, OpenAI).</param>
    /// <param name="cards">Репозиторій для збереження флеш-карток.</param>
    public TestGenerationService(IQuizGenerator ai, IFlashcardRepository cards)
    {
        this._ai = ai;
        this._cards = cards;
    }

    /// <summary>
    /// Асинхронно створює та зберігає флеш-картки на основі наданого тексту.
    /// </summary>
    /// <param name="creatorId">ID користувача, який створює картки.</param>
    /// <param name="text">Вхідний текст для аналізу та генерації карток.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="FlashcardDTO"/> новостворених флеш-карток.</returns>
    public async Task<IReadOnlyList<FlashcardDTO>> CreateFlashcardsFromTextAsync(
        int creatorId, string text, CancellationToken ct)
    {
        var items = await this._ai.GenerateFromTextAsync(text, ct);
        var result = new List<FlashcardDTO>();
        foreach (var (q, a, tags) in items)
        {
            var saved = await this._cards.AddAsync(
                new Flashcard
                {
                    Question = q,
                    Answer = a,
                    CreatorId = creatorId,
                    CreatedAt = DateTime.UtcNow,
                }, tags,
                ct);

            result.Add(saved.ToDTO(tags));
        }

        return result;
    }
}
