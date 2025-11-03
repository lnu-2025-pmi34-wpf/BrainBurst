namespace BrainBurst.BLL.DTO;

public sealed class TestDTO
{
    public int Id { get; init; }
    public int CreatorId { get; init; }
    public IReadOnlyList<FlashcardDTO> Questions { get; init; } = Array.Empty<FlashcardDTO>();
}