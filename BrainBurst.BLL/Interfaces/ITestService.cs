namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, що керує логікою тестів (створення, отримання, проходження).
/// </summary>
public interface ITestService
{
    /// <summary>
    /// Асинхронно генерує (створює) новий тест на основі списку ID флеш-карток.
    /// </summary>
    /// <param name="creatorId">ID користувача, який створює тест.</param>
    /// <param name="flashcardIds">Список ID флеш-карток, що увійдуть до тесту.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO створеного <see cref="TestDTO"/>.</returns>
    Task<TestDTO> GenerateFromFlashcardsAsync(int creatorId, IEnumerable<int> flashcardIds, CancellationToken ct);

    /// <summary>
    /// Асинхронно отримує тест за його ID.
    /// </summary>
    /// <param name="id">ID тесту для отримання.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO знайденого <see cref="TestDTO"/> або null, якщо не знайдено.</returns>
    Task<TestDTO?> GetAsync(int id, CancellationToken ct);

    /// <summary>
    /// Асинхронно приймає відповіді користувача на тест, перевіряє їх та зберігає результат.
    /// </summary>
    /// <param name="testId">ID тесту, що проходиться.</param>
    /// <param name="userId">ID користувача, який проходить тест.</param>
    /// <param name="answers">Список відповідей користувача у форматі (ID флеш-картки, відповідь).</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO з повним <see cref="TestResultDTO"/>.</returns>
    Task<TestResultDTO> SubmitAsync(int testId, int userId, IReadOnlyList<(int flashcardId, string? userInput)> answers, CancellationToken ct);
}
