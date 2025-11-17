namespace BrainBurst.BLL.DTO;

/// <summary>
/// Об'єкт передачі даних (DTO) для представлення результату відповіді на одне питання.
/// </summary>
public sealed class QuestionResultDTO
{
    /// <summary>
    /// Gets отримує або ініціалізує унікальний ідентифікатор результату.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує ID флеш-картки, на яку давалася відповідь.
    /// </summary>
    public int FlashcardId { get; init; }

    /// <summary>
    /// Gets отримує або ініціалізує відповідь, надану користувачем.
    /// </summary>
    public string UserInput { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether отримує або ініціалізує значення, що вказує, чи була відповідь правильною.
    /// </summary>
    public bool IsCorrect { get; init; }
}