using BrainBurst.DAL.Entities;
using BrainBurst.BLL.Mapping;
namespace BrainBurst.BLL.Services;

public sealed class TestGenerationService : ITestGenerationService
{
    private readonly IQuizGenerator _ai;
    private readonly IFlashcardRepository _cards;

    public TestGenerationService(IQuizGenerator ai, IFlashcardRepository cards)
    { _ai = ai; _cards = cards; }

    public async Task<IReadOnlyList<FlashcardDTO>> CreateFlashcardsFromTextAsync(
        int creatorId, string text, CancellationToken ct)
    {
        var items = await _ai.GenerateFromTextAsync(text, ct);
        var result = new List<FlashcardDTO>();
        foreach (var (q, a, tags) in items)
        {
            var saved = await _cards.AddAsync(new Flashcard
            {
                Question = q,
                Answer = a,
                CreatorId = creatorId,
                CreatedAt = DateTime.UtcNow
            }, tags, ct);

            result.Add(saved.ToDTO(tags));
        }
        return result;
    }
}
