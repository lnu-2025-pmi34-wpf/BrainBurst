namespace BrainBurst.BLL.DTO;

/// <summary>
/// Об'єкт передачі даних (DTO) для представлення запису в архіві тестів користувача.
/// </summary>
public sealed class ArchiveEntryDTO
{
    /// <summary>
    /// Gets отримує або ініціалізує ID результату тесту.
    /// </summary>
    public int TestResultId { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує назву тесту для відображення.
    /// </summary>
    public string TestTitle { get; init; } = string.Empty; // Назва тесту для відображення

    /// <summary>
    /// Gets отримує або ініціалізує відсоток правильних відповідей.
    /// </summary>
    public double CorrectAnswersPercent { get; init; } // Відсоток правильних відповідей

    /// <summary>
    /// Gets отримує або ініціалізує кількість балів, отриманих за тест.
    /// </summary>
    public int Points { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує дату та час проходження тесту.
    /// </summary>
    public DateTime TestDate { get; init; }
}