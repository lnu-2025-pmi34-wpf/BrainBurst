namespace BrainBurst.BLL.DTO;

using BrainBurst.BLL.Enums;

/// <summary>
/// Об'єкт передачі даних (DTO) для представлення інформації про користувача.
/// </summary>
public sealed class UserDTO
{
    /// <summary>
    /// Gets отримує або ініціалізує унікальний ідентифікатор користувача.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує адресу електронної пошти користувача.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets отримує або встановлює повне ім'я користувача.
    /// </summary>
    public string FullName { get; set; } = string.Empty; // <-- Змінено init на set

    /// <summary>
    /// Gets отримує або ініціалізує загальну кількість балів (рейтинг) користувача.
    /// </summary>
    public int Points { get; init; }

    /// <summary>
    /// Gets or sets отримує або встановлює ранг користувача <see cref="UserRank"/>.
    /// </summary>
    public UserRank Rank { get; set; }

    /// <summary>
    /// Gets or sets отримує або встановлює текстову мітку для рангу (наприклад, "Початківець" або "🏆").
    /// </summary>
    public string? RankLabel { get; set; }

    /// <summary>
    /// Gets отримує або ініціалізує дату та час створення облікового запису.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}