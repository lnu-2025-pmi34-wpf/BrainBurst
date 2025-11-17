using BrainBurst.BLL.Mapping;
using BrainBurst.DAL.Entities;

namespace BrainBurst.BLL.Services;

public sealed class TestService : ITestService
{
    private readonly ITestRepository _tests;
    private readonly ITestResultRepository _results;
    private readonly IUserRepository _users;
    private readonly IFlashcardRepository _cards;
    private readonly IRatingService _rating;

    public TestService(ITestRepository tests, ITestResultRepository results, IUserRepository users,
                       IFlashcardRepository cards, IRatingService rating)
    {
        this._tests = tests; this._results = results; this._users = users; this._cards = cards; this._rating = rating;
    }

    public async Task<TestDTO> GenerateFromFlashcardsAsync(int creatorId, IEnumerable<int> flashcardIds, CancellationToken ct)
    {
        if (flashcardIds == null || !flashcardIds.Any())
            throw new ArgumentException("Потрібен хоча б один flashcardId.");

        var test = await this._tests.CreateFromFlashcardsAsync(creatorId, flashcardIds, ct);

        var allCards = await this._cards.FindAsync(creatorId, null, ct);
        var set = allCards.Where(c => flashcardIds.Contains(c.FlashcardId)).ToList();

        return set.ToTestDTO(test.TestId, creatorId);
    }

    public async Task<TestDTO?> GetAsync(int id, CancellationToken ct)
    {
        var t = await this._tests.GetAsync(id, ct);
        if (t is null) return null;

        return new TestDTO
        {
            Id = t.TestId,
            CreatorId = t.CreatorId,
            Questions = Array.Empty<FlashcardDTO>()
        };
    }


    public async Task<TestResultDTO> SubmitAsync(
        int testId, int userId,
        IReadOnlyList<(int flashcardId, string userInput)> answers,
        CancellationToken ct)
    {
        if (answers.Count == 0) throw new ArgumentException("Відповіді відсутні.");

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

            if (isCorrect) correct++;

            qr.Add(new QuestionResult
            {
                FlashcardId = fid,
                UserInput = input ?? string.Empty,
                IsCorrect = isCorrect
            });
        }

        double percent = 100.0 * correct / answers.Count;
        int points = correct * 10 + (Math.Abs(percent - 100.0) < double.Epsilon ? 20 : 0);

        var tr = new TestResult
        {
            TestId = testId,
            UserId = userId,
            CorrectAnswersPercent = (decimal)percent,
            Points = points,
            TestDate = DateTime.UtcNow
        };


        var saved = await this._results.AddAsync(tr, qr, ct);

        user.Points += points;
        await this._users.UpdateAsync(user, ct);

        var dto = saved.ToDTO(qr.Select(q => q.ToDTO()));
        return dto;
    }
}