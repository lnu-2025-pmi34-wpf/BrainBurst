namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, що відповідає за отримання архіву тестів.
/// </summary>
public interface IArchiveService
{
    /// <summary>
    /// Асинхронно отримує архів пройдених тестів для конкретного користувача.
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача, чий архів потрібно отримати.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="ArchiveEntryDTO"/>, доступний лише для читання.</returns>
    Task<IReadOnlyList<ArchiveEntryDTO>> GetArchiveAsync(int userId, CancellationToken ct);
}