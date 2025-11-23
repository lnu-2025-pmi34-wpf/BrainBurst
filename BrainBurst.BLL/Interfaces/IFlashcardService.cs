namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, що керує логікою флеш-карток.
/// </summary>
public interface IFlashcardService
{
    /// <summary>
    /// Асинхронно створює нову флеш-картку.
    /// </summary>
    /// <param name="creatorId">ID користувача, який створює картку.</param>
    /// <param name="question">Текст питання.</param>
    /// <param name="answer">Текст відповіді.</param>
    /// <param name="tags">Список тегів для картки.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO створеної <see cref="FlashcardDTO"/>.</returns>
    Task<FlashcardDTO> CreateAsync(int creatorId, string question, string answer, IEnumerable<string> tags, CancellationToken ct);

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
    Task<FlashcardDTO> UpdateAsync(int id, int editorId, string question, string answer, IEnumerable<string> tags, CancellationToken ct);

    /// <summary>
    /// Асинхронно видаляє флеш-картку.
    /// </summary>
    /// <param name="id">ID картки, яку потрібно видалити.</param>
    /// <param name="requesterId">ID користувача, який запитує видалення (для перевірки прав).</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
    Task DeleteAsync(int id, int requesterId, CancellationToken ct);

    /// <summary>
    /// Асинхронно отримує одну флеш-картку за її ID.
    /// </summary>
    /// <param name="id">ID картки для отримання.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO знайденої <see cref="FlashcardDTO"/> або null, якщо не знайдено.</returns>
    Task<FlashcardDTO?> GetAsync(int id, CancellationToken ct);

    /// <summary>
    /// Асинхронно отримує список флеш-карток, що належать користувачу.
    /// </summary>
    /// <param name="ownerId">ID користувача, чиї картки потрібно знайти.</param>
    /// <param name="search">Опційний пошуковий рядок для фільтрації.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="FlashcardDTO"/>, доступний лише для читання.</returns>
    Task<IReadOnlyList<FlashcardDTO>> ListAsync(int ownerId, string? search, CancellationToken ct);
}