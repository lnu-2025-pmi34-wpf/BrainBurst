namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, що відповідає за автентифікацію та реєстрацію користувачів.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Асинхронно реєструє нового користувача в системі.
    /// </summary>
    /// <param name="email">Адреса електронної пошти.</param>
    /// <param name="password">Пароль (у відкритому вигляді).</param>
    /// <param name="fullName">Повне ім'я користувача.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO створеного <see cref="UserDTO"/>.</returns>
    Task<UserDTO> RegisterAsync(string email, string password, string fullName, CancellationToken ct);

    /// <summary>
    /// Асинхронно автентифікує користувача в системі.
    /// </summary>
    /// <param name="email">Адреса електронної пошти.</param>
    /// <param name="password">Пароль (у відкритому вигляді).</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>DTO автентифікованого <see cref="UserDTO"/>.</returns>
    Task<UserDTO> LoginAsync(string email, string password, CancellationToken ct);
}