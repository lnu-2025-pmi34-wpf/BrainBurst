namespace BrainBurst.BLL.DTO;

public sealed class FlashcardDTO
{
    public int Id { get; init; }
    public string Question { get; init; } = "";
    public string Answer { get; init; } = "";
    public int CreatorId { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}