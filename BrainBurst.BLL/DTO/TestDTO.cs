namespace BrainBurst.BLL.DTO;

/// <summary>
/// Об'єкт передачі даних (DTO) для представлення тесту, що містить набір питань.
/// </summary>
public sealed class TestDTO
{
    /// <summary>
    /// Gets отримує або ініціалізує унікальний ідентифікатор тесту.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує ID користувача, який створив тест.
    /// </summary>
    public int CreatorId { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує список питань (флеш-карток) у цьому тесті.
    /// </summary>
    public IReadOnlyList<FlashcardDTO> Questions { get; init; } = Array.Empty<FlashcardDTO>();
}