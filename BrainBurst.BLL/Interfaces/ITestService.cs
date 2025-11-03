namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

public interface ITestService
{
    Task<TestDTO> GenerateFromFlashcardsAsync(int creatorId, IEnumerable<int> flashcardIds, CancellationToken ct);
    Task<TestDTO?> GetAsync(int id, CancellationToken ct);
    Task<TestResultDTO> SubmitAsync(int testId, int userId, IReadOnlyList<(int flashcardId, string userInput)> answers, CancellationToken ct);
}
