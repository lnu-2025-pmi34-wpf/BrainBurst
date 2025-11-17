namespace BrainBurst.BLL.DTO;

/// <summary>
/// Об'єкт передачі даних (DTO) для представлення флеш-картки.
/// </summary>
public sealed class FlashcardDTO
{
    /// <summary>
    /// Gets отримує або ініціалізує унікальний ідентифікатор флеш-картки.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує текст питання.
    /// </summary>
    public string Question { get; init; } = string.Empty;

    /// <summary>
    /// Gets отримує або ініціалізує текст відповіді.
    /// </summary>
    public string Answer { get; init; } = string.Empty;

    /// <summary>
    /// Gets отримує або ініціалізує ID користувача, який створив картку.
    /// </summary>
    public int CreatorId { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує дату та час створення картки.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує список тегів, пов'язаних з карткою (лише для читання).
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}