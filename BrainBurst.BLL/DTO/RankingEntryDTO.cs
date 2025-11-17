namespace BrainBurst.BLL.DTO;

/// <summary>
/// Об'єкт передачі даних (DTO) для представлення одного запису у рейтингу користувачів.
/// </summary>
public sealed class RankingEntryDTO
{
    /// <summary>
    /// Gets отримує або ініціалізує унікальний ідентифікатор користувача.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує повне ім'я користувача.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Gets отримує або ініціалізує загальну кількість балів користувача.
    /// </summary>
    public int Points { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує ранг користувача (наприклад, "Початківець").
    /// </summary>
    public string Rank { get; init; } = string.Empty;
}