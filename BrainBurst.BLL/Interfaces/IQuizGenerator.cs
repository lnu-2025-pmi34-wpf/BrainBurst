namespace BrainBurst.BLL.Interfaces.Abstractions;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, здатного генерувати пари "питання-відповідь" (квізи) з тексту.
/// </summary>
public interface IQuizGenerator
{
    /// <summary>
    /// Асинхронно генерує список пар "питання-відповідь" та тегів на основі наданого тексту.
    /// </summary>
    /// <param name="text">Вхідний текст, з якого потрібно згенерувати флеш-картки.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список кортежів, що містять (Питання, Відповідь, Список тегів).</returns>
    Task<IReadOnlyList<(string Question, string Answer, IReadOnlyList<string> Tags)>> GenerateFromTextAsync(
        string text, CancellationToken ct);
}