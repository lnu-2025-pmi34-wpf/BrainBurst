namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, що керує логікою, пов'язаною з профілями користувачів.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Асинхронно видаляє обліковий запис користувача.
    /// </summary>
    /// <param name="userId">ID користувача для видалення.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
    Task DeleteAccountAsync(int userId, CancellationToken ct); // НОВИЙ МЕТОД

    /// <summary>
    /// Асинхронно отримує DTO користувача за його ID.
    /// </summary>
    /// <param name="id">ID користувача для отримання.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO <see cref="UserDTO"/>.</returns>
    Task<UserDTO> GetAsync(int id, CancellationToken ct);

    /// <summary>
    /// Асинхронно оновлює профіль користувача (наприклад, повне ім'я).
    /// </summary>
    /// <param name="id">ID користувача, чий профіль оновлюється.</param>
    /// <param name="fullName">Нове повне ім'я.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Оновлений DTO <see cref="UserDTO"/>.</returns>
    Task<UserDTO> UpdateProfileAsync(int id, string fullName, CancellationToken ct);

    /// <summary>
    /// Асинхронно отримує таблицю лідерів (рейтинг).
    /// </summary>
    /// <param name="top">Кількість користувачів, яку потрібно повернути.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="RankingEntryDTO"/>, доступний лише для читання.</returns>
    Task<IReadOnlyList<RankingEntryDTO>> GetLeaderboardAsync(int top, CancellationToken ct);

    /// <summary>
    /// Асинхронно змінює пароль користувача.
    /// </summary>
    /// <param name="userId">ID користувача, який змінює пароль.</param>
    /// <param name="oldPassword">Поточний (старий) пароль.</param>
    /// <param name="newPassword">Новий пароль.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct);
}