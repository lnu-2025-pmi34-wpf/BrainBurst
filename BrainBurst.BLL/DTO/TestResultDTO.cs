namespace BrainBurst.BLL.DTO;

/// <summary>
/// Об'єкт передачі даних (DTO) для представлення повного результату проходження тесту.
/// </summary>
public sealed class TestResultDTO
{
    /// <summary>
    /// Gets отримує або ініціалізує унікальний ідентифікатор результату тесту.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує ID тесту, до якого відноситься цей результат.
    /// </summary>
    public int TestId { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує ID користувача, який пройшов тест.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує відсоток правильних відповідей (від 0 до 100).
    /// </summary>
    public double CorrectAnswersPercent { get; init; } // 0..100

    /// <summary>
    /// Gets отримує або ініціалізує кількість балів, отриманих за тест.
    /// </summary>
    public int Points { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує дату та час проходження тесту.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує список детальних результатів по кожному питанню.
    /// </summary>
    public IReadOnlyList<QuestionResultDTO> Questions { get; init; } = Array.Empty<QuestionResultDTO>();
}