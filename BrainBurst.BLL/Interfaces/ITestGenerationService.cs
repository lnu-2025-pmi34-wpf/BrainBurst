namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає операції для роботи з тестами та генерацією флеш-карток з тексту.
/// </summary>
public interface ITestGenerationService
{
    /// <summary>
    /// Асинхронно створює та зберігає флеш-картки на основі наданого тексту.
    /// Теги передаються окремо від користувача через UI.
    /// </summary>
    /// <param name="creatorId">ID користувача, який створює картки.</param>
    /// <param name="text">Вхідний текст для аналізу та генерації карток.</param>
    /// <param name="tags">Список тегів, які призначити всім створеним карткам.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="FlashcardDTO"/> новостворених флеш-карток.</returns>
    Task<IReadOnlyList<FlashcardDTO>> CreateFlashcardsFromTextAsync(
        int creatorId, string text, IEnumerable<string> tags, CancellationToken ct);
}