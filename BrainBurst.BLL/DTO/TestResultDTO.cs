namespace BrainBurst.BLL.DTO;

public sealed class TestResultDTO
{
    public int Id { get; init; }
    public int TestId { get; init; }
    public int UserId { get; init; }
    public double CorrectAnswersPercent { get; init; } // 0..100
    public int Points { get; init; }
    public DateTime Date { get; init; }
    public IReadOnlyList<QuestionResultDTO> Questions { get; init; } = Array.Empty<QuestionResultDTO>();
}