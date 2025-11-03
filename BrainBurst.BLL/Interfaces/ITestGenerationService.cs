namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

public interface ITestGenerationService
{
    Task<IReadOnlyList<FlashcardDTO>> CreateFlashcardsFromTextAsync(int creatorId, string text, CancellationToken ct);
}