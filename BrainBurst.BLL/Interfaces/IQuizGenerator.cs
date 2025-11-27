namespace BrainBurst.BLL.Interfaces.Abstractions;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, здатного генерувати пари "питання-відповідь" (квізи) з тексту.
/// </summary>
public interface IQuizGenerator
{
    /// <summary>
    /// Асинхронно генерує список пар "питання-відповідь" на основі наданого тексту.
    /// Тегирування здійснює користувач окремо через UI.
    /// </summary>
    /// <param name="text">Вхідний текст, з якого потрібно згенерувати флеш-картки.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список кортежів, що містять (Питання, Відповідь).</returns>
    Task<IReadOnlyList<(string Question, string Answer)>> GenerateFromTextAsync(
        string text, CancellationToken ct);
}