namespace BrainBurst.BLL.Services;
using BrainBurst.BLL.Mapping;
using Microsoft.Extensions.Logging;

/// <summary>
/// Реалізація сервісу, що керує логікою тестів (створення, отримання, проходження).
/// </summary>
public sealed class TestService : ITestService
{
    private readonly ITestRepository _tests;
    private readonly ITestResultRepository _results;
    private readonly IUserRepository _users;
    private readonly IFlashcardRepository _cards;
    private readonly IRatingService _rating;
    private readonly ILogger<TestService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestService"/> class.
    /// </summary>
    /// <param name="tests">Репозиторій для доступу до тестів.</param>
    /// <param name="results">Репозиторій для доступу до результатів тестів.</param>
    /// <param name="users">Репозиторій для доступу до користувачів.</param>
    /// <param name="cards">Репозиторій для доступу до флеш-карток.</param>
    /// <param name="rating">Сервіс для роботи з рейтингами.</param>
    /// <param name="logger">Логер для запису подій.</param>
    public TestService(ITestRepository tests, ITestResultRepository results, IUserRepository users, IFlashcardRepository cards, IRatingService rating, ILogger<TestService> logger)
    {
        this._tests = tests;
        this._results = results;
        this._users = users;
        this._cards = cards;
        this._rating = rating;
        this._logger = logger;

        this._logger.LogDebug("TestService: Сервіс тестів ініціалізовано.");
    }

    /// <summary>
    /// Асинхронно генерує (створює) новий тест на основі списку ID флеш-карток.
    /// </summary>
    /// <param name="creatorId">ID користувача, який створює тест.</param>
    /// <param name="flashcardIds">Список ID флеш-карток, що увійдуть до тесту.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO створеного <see cref="TestDTO"/>.</returns>
    /// <exception cref="ArgumentException">Виникає, якщо список `flashcardIds` порожній.</exception>
    public async Task<TestDTO> GenerateFromFlashcardsAsync(int creatorId, IEnumerable<int> flashcardIds, CancellationToken ct)
    {
        var cardCount = flashcardIds?.Count() ?? 0;
        this._logger.LogInformation("GenerateFromFlashcardsAsync: Спроба створити тест для {CreatorId} з {Count} карток.", creatorId, cardCount);

        try
        {
            if (flashcardIds == null || !flashcardIds.Any())
            {
                this._logger.LogWarning("GenerateFromFlashcardsAsync: Спроба створити тест з порожнім списком карток.");
                throw new ArgumentException("Потрібен хоча б один flashcardId.");
            }

            var test = await this._tests.CreateFromFlashcardsAsync(creatorId, flashcardIds, ct);

            var allCards = await this._cards.FindAsync(creatorId, null, ct);
            var set = allCards.Where(c => flashcardIds.Contains(c.FlashcardId)).ToList();

            this._logger.LogInformation("GenerateFromFlashcardsAsync: Тест {TestId} успішно створено з {Count} питань.", test.TestId, set.Count);

            return set.ToTestDTO(test.TestId, creatorId);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "GenerateFromFlashcardsAsync: Критична помилка при генерації тесту для {CreatorId}.", creatorId);
            throw;
        }
    }

    /// <summary>
    /// Асинхронно отримує тест за його ID.
    /// </summary>
    /// <param name="id">ID тесту для отримання.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO знайденого <see cref="TestDTO"/> (без питань) або null, якщо не знайдено.</returns>
    public async Task<TestDTO?> GetAsync(int id, CancellationToken ct)
    {
        this._logger.LogDebug("GetAsync: Спроба отримати тест за ID: {TestId}", id);

        try
        {
            var t = await this._tests.GetAsync(id, ct);
            if (t is null)
            {
                this._logger.LogWarning("GetAsync: Тест {TestId} не знайдено.", id);
                return null;
            }

            this._logger.LogDebug("GetAsync: Тест {TestId} знайдено.", id);

            return new TestDTO
            {
                Id = t.TestId,
                CreatorId = t.CreatorId,
                Questions = Array.Empty<FlashcardDTO>(),
            };
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "GetAsync: Критична помилка при отриманні тесту {TestId}.", id);
            throw;
        }
    }

    /// <summary>
    /// Асинхронно приймає відповіді користувача на тест, перевіряє їх та зберігає результат.
    /// </summary>
    /// <param name="testId">ID тесту, що проходиться.</param>
    /// <param name="userId">ID користувача, який проходить тест.</param>
    /// <param name="answers">Список відповідей користувача у форматі (ID флеш-картки, відповідь).</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO з повним <see cref="TestResultDTO"/>.</returns>
    /// <exception cref="ArgumentException">Виникає, якщо список `answers` порожній.</exception>
    /// <exception cref="InvalidOperationException">Виникає, якщо тест не знайдено.</exception>
    /// <exception cref="KeyNotFoundException">Виникає, якщо користувача не знайдено.</exception>
    public async Task<TestResultDTO> SubmitAsync(int testId, int userId, IReadOnlyList<(int flashcardId, string? userInput)> answers, CancellationToken ct)
    {
        this._logger.LogInformation("SubmitAsync: Спроба подати результати тесту {TestId} користувачем {UserId}. Кількість відповідей: {Count}", testId, userId, answers.Count);

        try
        {
            if (answers.Count == 0)
            {
                this._logger.LogWarning("SubmitAsync: Відповіді відсутні для тесту {TestId}.", testId);
                throw new ArgumentException("Відповіді відсутні.");
            }

            var test = await this._tests.GetAsync(testId, ct) ?? throw new InvalidOperationException("Тест не знайдено.");
            var user = await this._users.GetByIdAsync(userId, ct);

            var cards = await this._cards.FindAsync(test.CreatorId, null, ct);
            var cardById = cards.ToDictionary(c => c.FlashcardId);

            int correct = 0;
            var qr = new List<QuestionResult>();
            foreach (var (fid, input) in answers)
            {
                var isCorrect = cardById.TryGetValue(fid, out var fc) &&
                                string.Equals(fc.Answer?.Trim(), (input ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    correct++;
                }

                qr.Add(new QuestionResult
                {
                    FlashcardId = fid,
                    UserInput = input ?? string.Empty,
                    IsCorrect = isCorrect,
                });
            }

            double percent = 100.0 * correct / answers.Count;
            int points = (correct * 10) + (Math.Abs(percent - 100.0) < double.Epsilon ? 20 : 0);

            this._logger.LogInformation("SubmitAsync: Тест {TestId} оцінено. Правильно: {Correct}, %: {Percent:N2}, Балів: {Points}",
                                       testId, correct, percent, points);

            var tr = new TestResult
            {
                TestId = testId,
                UserId = userId,
                CorrectAnswersPercent = (decimal)percent,
                Points = points,
                TestDate = DateTime.UtcNow,
            };

            var saved = await this._results.AddAsync(tr, qr, ct);

            user.Points += points;
            await this._users.UpdateAsync(user, ct);

            this._logger.LogInformation("SubmitAsync: Бали користувача {UserId} оновлено. Додано {Points} балів.", userId, points);

            var dto = saved.ToDTO(qr.Select(q => q.ToDTO()));
            return dto;
        }
        catch (Exception ex)
        {
            if (ex is not ArgumentException && ex is not InvalidOperationException && ex is not KeyNotFoundException)
            {
                this._logger.LogError(ex, "SubmitAsync: Критична помилка при поданні результатів тесту {TestId} користувачем {UserId}.", testId, userId);
            }

            throw;
        }
    }
}