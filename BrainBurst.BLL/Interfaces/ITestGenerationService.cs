namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, що відповідає за генерацію флеш-карток з тексту.
/// </summary>
public interface ITestGenerationService
{
    /// <summary>
    /// Асинхронно створює та зберігає флеш-картки на основі наданого тексту.
    /// </summary>
    /// <param name="creatorId">ID користувача, який створює картки.</param>
    /// <param name="text">Вхідний текст для аналізу та генерації карток.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="FlashcardDTO"/> новостворених флеш-карток.</returns>
    Task<IReadOnlyList<FlashcardDTO>> CreateFlashcardsFromTextAsync(int creatorId, string text, CancellationToken ct);
}