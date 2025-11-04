namespace BrainBurst.BLL.DTO;

public sealed class ArchiveEntryDTO
{
    public int TestResultId { get; init; }
    public string TestTitle { get; init; } = ""; // Назва тесту для відображення
    public double CorrectAnswersPercent { get; init; } // Відсоток правильних відповідей
    public int Points { get; init; }
    public DateTime TestDate { get; init; }
}