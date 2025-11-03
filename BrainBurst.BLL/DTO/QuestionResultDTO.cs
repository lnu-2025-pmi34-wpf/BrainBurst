namespace BrainBurst.BLL.DTO;

public sealed class QuestionResultDTO
{
    public int Id { get; init; }
    public int FlashcardId { get; init; }
    public string UserInput { get; init; } = "";
    public bool IsCorrect { get; init; }
}